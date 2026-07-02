using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PCL_CE.Neo.Core.Minecraft.Parser;
using PCL_CE.Neo.Core.Minecraft.Scanner;

namespace PCL_CE.Neo.Core.Minecraft;

/// <summary>
/// Java 来源类型
/// </summary>
public enum JavaSource
{
    /// <summary>
    /// 自动扫描发现
    /// </summary>
    AutoScanned,
    
    /// <summary>
    /// 手动添加
    /// </summary>
    ManualAdded,
    
    /// <summary>
    /// 从配置恢复
    /// </summary>
    FromConfig
}

/// <summary>
/// Java 条目，包含安装信息、启用状态和来源
/// </summary>
public sealed class JavaEntry
{
    /// <summary>
    /// Java 安装信息
    /// </summary>
    public JavaInstallation Installation { get; set; } = null!;
    
    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsEnabled { get; set; }
    
    /// <summary>
    /// 来源类型
    /// </summary>
    public JavaSource Source { get; set; }
}

/// <summary>
/// Java 存储项（用于配置序列化）
/// </summary>
public sealed class JavaStorageItem
{
    public string Path { get; set; } = "";
    public bool IsEnable { get; set; }
    public JavaSource Source { get; set; }
}

/// <summary>
/// Java 管理器，负责 Java 安装的扫描、管理和选择
/// </summary>
public class JavaManager : IJavaManager
{
    private readonly ILogger<JavaManager>? _logger;
    private readonly IJavaParser _parser;
    private readonly List<IJavaScannerStrategy> _scanners;
    
    private readonly Dictionary<string, JavaEntry> _javaEntries = new();
    private readonly SemaphoreSlim _scanLock = new(1, 1);
    private DateTime _lastScanTime = DateTime.MinValue;
    private static readonly TimeSpan MinScanInterval = TimeSpan.FromSeconds(13);

    public JavaManager() : this(null, new PeHeaderParser(), new List<IJavaScannerStrategy>())
    {
    }

    public JavaManager(
        ILogger<JavaManager>? logger,
        IJavaParser parser,
        List<IJavaScannerStrategy> scanners)
    {
        _logger = logger;
        _parser = parser;
        _scanners = scanners;
    }

    /// <summary>
    /// 创建包含默认扫描器的 JavaManager
    /// </summary>
    public static JavaManager CreateDefault(ILogger<JavaManager>? logger = null)
    {
        var parser = new PeHeaderParser();
        var scanners = new List<IJavaScannerStrategy>
        {
            new RegistryJavaScanner(),
            new DefaultPathsScanner(),
            new PathEnvironmentScanner()
        };
        
        return new JavaManager(logger, parser, scanners);
    }

    /// <summary>
    /// 保存配置到 JSON
    /// </summary>
    public string SaveConfig()
    {
        try
        {
            var items = _javaEntries
                .Select(x => new JavaStorageItem
                {
                    Path = x.Value.Installation.JavaExePath,
                    IsEnable = x.Value.IsEnabled,
                    Source = x.Value.Source
                })
                .ToList();
            
            return JsonSerializer.Serialize(items, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "保存 Java 配置失败");
            return "";
        }
    }

    /// <summary>
    /// 从 JSON 读取配置
    /// </summary>
    public void ReadConfig(string jsonConfig)
    {
        try
        {
            if (string.IsNullOrEmpty(jsonConfig))
                return;
            
            var items = JsonSerializer.Deserialize<List<JavaStorageItem>>(jsonConfig);
            if (items == null) return;

            var itemsAdded = new List<JavaEntry>();

            foreach (var item in items)
            {
                var installation = _parser.Parse(item.Path);
                if (installation == null)
                {
                    _logger?.LogDebug("无法找到 Java: {Path}", item.Path);
                    continue;
                }

                itemsAdded.Add(new JavaEntry
                {
                    Installation = installation,
                    IsEnabled = item.IsEnable,
                    Source = item.Source != default ? item.Source : JavaSource.AutoScanned
                });
            }

            lock (_javaEntries)
            {
                foreach (var item in itemsAdded)
                {
                    if (_javaEntries.TryGetValue(item.Installation.JavaExePath, out var existingRecord))
                    {
                        existingRecord.IsEnabled = item.IsEnabled;
                        existingRecord.Source = item.Source;
                    }
                    else
                    {
                        _javaEntries.Add(item.Installation.JavaExePath, item);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "读取 Java 配置失败");
        }
    }

    /// <summary>
    /// 扫描 Java 安装
    /// </summary>
    /// <param name="force">强制扫描，忽略时间间隔限制</param>
    public async Task ScanJavaAsync(bool force = false)
    {
        if (ShouldSkipScan(force)) return;

        if (!await _scanLock.WaitAsync(TimeSpan.FromSeconds(7)))
        {
            _logger?.LogWarning("扫描锁等待超时，跳过本次扫描");
            return;
        }

        try
        {
            if (ShouldSkipScan(force)) return;

            await Task.Run(ScanInternal);
            _lastScanTime = DateTime.Now;
            
            var logInfo = string.Join("\n\t", GetSortedJavaList().Select(j => j.ToString()));
            _logger?.LogInformation("Java 扫描完成: \n\t{Info}", logInfo);
        }
        finally
        {
            _scanLock.Release();
        }
    }

    private bool ShouldSkipScan(bool force)
    {
        return !force && (DateTime.Now - _lastScanTime) < MinScanInterval;
    }

    private void ScanInternal()
    {
        var pathSet = new ConcurrentDictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

        // 并行执行所有扫描器
        Parallel.ForEach(_scanners, scanner =>
        {
            var temp = new List<string>();
            scanner.Scan(temp);
            
            foreach (var path in temp)
            {
                var normalized = NormalizePath(path);
                if (!ShouldExcludePath(normalized))
                    pathSet.TryAdd(normalized, true);
            }
        });

        // 解析所有找到的路径
        var scannedEntries = pathSet.Keys
            .Select(path => _parser.Parse(path))
            .Where(inst => inst != null)
            .Select(inst => new JavaEntry
            {
                Installation = inst!,
                IsEnabled = _javaEntries.TryGetValue(NormalizePath(inst!.JavaExePath), out var existingJava)
                    ? existingJava.IsEnabled
                    : ShouldEnableByDefault(inst!),
                Source = JavaSource.AutoScanned
            })
            .ToList();

        lock (_javaEntries)
        {
            foreach (var entry in scannedEntries)
            {
                _javaEntries[entry.Installation.JavaExePath] = entry;
            }
        }
    }

    /// <summary>
    /// 判断是否应该默认启用
    /// </summary>
    private static bool ShouldEnableByDefault(JavaInstallation inst)
    {
        // 检查是否可用
        var libDir = Path.Combine(Directory.GetParent(inst.JavaFolder)?.FullName ?? inst.JavaFolder, "lib");
        var isUsable = (!inst.IsJre && File.Exists(Path.Combine(libDir, "jvm.lib"))) ||
                       (inst.IsJre && File.Exists(Path.Combine(libDir, "rt.jar")));

        // JRE 版本 > 8 不启用
        if (inst.IsJre && inst.MajorVersion > 8)
            return false;

        // 架构不匹配不启用
        if (inst.Is64Bit != Environment.Is64BitOperatingSystem)
            return false;

        // 不可用不启用
        return isUsable;
    }

    /// <summary>
    /// 获取排序后的 Java 列表
    /// </summary>
    public List<JavaEntry> GetSortedJavaList()
    {
        var ret = _javaEntries.Values.ToList();
        ret.Sort((a, b) =>
        {
            // 先按版本排序
            var versionCmp = a.Installation.Version.CompareTo(b.Installation.Version);
            if (versionCmp != 0) return versionCmp;
            
            // 版本相同按品牌排序
            return a.Installation.Brand.CompareTo(b.Installation.Brand);
        });
        ret.Reverse();
        return ret;
    }

    /// <summary>
    /// 是否存在64位 Java
    /// </summary>
    public bool Existing64BitJava()
    {
        lock (_javaEntries)
        {
            return _javaEntries.Any(x => x.Value.Installation.Is64Bit);
        }
    }

    /// <summary>
    /// 是否存在任何 Java
    /// </summary>
    public bool ExistAnyJava()
    {
        return _javaEntries.Count != 0;
    }

    /// <summary>
    /// 是否存在指定路径的 Java
    /// </summary>
    public bool Exist(string javaExePath)
    {
        return _javaEntries.ContainsKey(NormalizePath(javaExePath));
    }

    /// <summary>
    /// 添加或获取 Java 条目
    /// </summary>
    public JavaEntry? AddOrGet(string javaExePath)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(javaExePath) || !File.Exists(javaExePath))
                return null;

            var installation = _parser.Parse(javaExePath);
            if (installation == null) return null;

            var exePath = NormalizePath(installation.JavaExePath);
            
            lock (_javaEntries)
            {
                if (_javaEntries.TryGetValue(exePath, out var ret))
                    return ret;

                var entry = new JavaEntry
                {
                    Installation = installation,
                    IsEnabled = ShouldEnableByDefault(installation),
                    Source = JavaSource.ManualAdded
                };

                _javaEntries.Add(exePath, entry);
                return entry;
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "添加 Java 失败: {Path}", javaExePath);
            return null;
        }
    }

    /// <summary>
    /// 仅获取 Java 条目（不存在则不添加）
    /// </summary>
    public JavaEntry? Get(string javaExePath)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(javaExePath) || !File.Exists(javaExePath))
                return null;

            var installation = _parser.Parse(javaExePath);
            if (installation == null) return null;

            var exePath = NormalizePath(installation.JavaExePath);
            
            lock (_javaEntries)
            {
                if (_javaEntries.TryGetValue(exePath, out var ret))
                    return ret;

                // 不存在也不添加，仅返回基本信息
                return new JavaEntry
                {
                    Installation = installation,
                    IsEnabled = ShouldEnableByDefault(installation),
                    Source = JavaSource.AutoScanned
                };
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "获取 Java 失败: {Path}", javaExePath);
            return null;
        }
    }

    /// <summary>
    /// 选择适合指定版本范围的 Java
    /// </summary>
    public async Task<JavaEntry[]> SelectSuitableJavaAsync(Version minVersion, Version maxVersion)
    {
        if (_javaEntries.Count == 0)
            await ScanJavaAsync();

        lock (_javaEntries)
        {
            return _javaEntries
                .Values
                .Where(j => j.Installation.IsStillAvailable && j.IsEnabled &&
                            IsVersionSuitable(j.Installation.Version, minVersion, maxVersion))
                .OrderBy(static j => j.Installation.MajorVersion)
                .ThenBy(static j => j.Installation.IsJre)
                .ThenBy(static j => j.Installation.Brand)
                .ThenByDescending(static j => j.Installation.Version)
                .ToArray();
        }
    }

    /// <summary>
    /// 检查所有 Java 的可用性
    /// </summary>
    public void CheckAllAvailability()
    {
        lock (_javaEntries)
        {
            var keysToRemove = _javaEntries
                .Where(kv => !kv.Value.Installation.IsStillAvailable)
                .Select(kv => kv.Key)
                .ToArray();

            foreach (var key in keysToRemove)
            {
                _javaEntries.Remove(key);
                _logger?.LogInformation("移除不可用的 Java: {Key}", key);
            }
        }
    }

    /// <summary>
    /// 将 Java 版本规范化为统一比较格式（1.8.0 → 8.0.0）
    /// </summary>
    public static Version NormalizeVersion(Version version)
    {
        return version.Major == 1 && version.Minor >= 0
            ? new Version(version.Minor, Math.Max(version.Build, 0), Math.Max(version.Revision, 0))
            : version;
    }

    /// <summary>
    /// 检查版本是否在指定范围内（闭区间）
    /// </summary>
    public static bool IsVersionSuitable(Version javaVersion, Version minVersion, Version maxVersion)
    {
        var normalizedJava = NormalizeVersion(javaVersion);
        var normalizedMin = NormalizeVersion(minVersion);
        var normalizedMax = NormalizeVersion(maxVersion);

        return normalizedJava >= normalizedMin && normalizedJava <= normalizedMax;
    }

    /// <summary>
    /// 规范化路径
    /// </summary>
    private static string NormalizePath(string path)
    {
        return Path.GetFullPath(path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar))
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
    }

    /// <summary>
    /// 判断是否应该排除路径
    /// </summary>
    private static bool ShouldExcludePath(string path)
    {
        var excludeNames = new[] { "javapath", "java8path", "common files", "netease" };
        return path.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            .Any(part => excludeNames.Contains(part, StringComparer.OrdinalIgnoreCase));
    }
}