using Microsoft.Extensions.Logging;
using PCL_CE.Neo.Core.Minecraft.Java;
using PCL_CE.Neo.Core.Minecraft.Java.Parser;
using PCL_CE.Neo.Core.Minecraft.Java.Scanner;
using PCL_CE.Neo.Core.Utils.Exts;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace PCL_CE.Neo.Core.Minecraft;

public class JavaManager
{
    private const string ModuleName = "JavaManager";
    private readonly Dictionary<string, JavaEntry> _javaEntrys = new();

    private readonly IJavaParser _parser;
    private readonly IJavaScanner[] _scanners;

    private readonly SemaphoreSlim _scanLock = new(1, 1);
    private DateTime _lastScanTime = DateTime.MinValue;
    private static readonly TimeSpan _MinScanInterval = TimeSpan.FromSeconds(13);

    private readonly ILogger<JavaManager> _logger;
    private readonly string _configPath;

    public JavaManager(
        ILogger<JavaManager> logger,
        IJavaParser parser,
        string configPath,
        params IJavaScanner[] scanners)
    {
        _logger = logger;
        _parser = parser;
        _configPath = configPath;
        _scanners = scanners;
    }

    public void SaveConfig()
    {
        try
        {
            var items = _javaEntrys
                .Select(x => new JavaStorageItem()
                {
                    Path = x.Value.Installation.JavaExePath,
                    IsEnable = x.Value.IsEnabled,
                    Source = x.Value.Source
                })
                .ToArray();

            var directory = Path.GetDirectoryName(_configPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(_configPath, JsonSerializer.Serialize(items));
            _logger.LogInformation("Java 配置已保存到 {Path}", _configPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "保存 Java 配置项失败");
        }
    }

    public void ReadConfig()
    {
        try
        {
            if (!File.Exists(_configPath)) return;

            var json = File.ReadAllText(_configPath);
            var items = JsonSerializer.Deserialize<JavaStorageItem[]>(json);
            if (items == null) return;

            var itemsAdded = new List<JavaEntry>();

            foreach (var item in items)
            {
                var parserResult = _parser.Parse(item.Path);
                if (parserResult == null)
                {
                    _logger.LogTrace("无法找到 Java {Path}，跳过", item.Path);
                    continue;
                }

                itemsAdded.Add(new JavaEntry()
                {
                    Installation = parserResult,
                    IsEnabled = item.IsEnable,
                    Source = item.Source ?? JavaSource.AutoScanned
                });
            }

            lock (_javaEntrys)
            {
                foreach (var item in itemsAdded)
                {
                    if (_javaEntrys.TryGetValue(item.Installation.JavaExePath, out var existingRecord))
                    {
                        existingRecord.IsEnabled = item.IsEnabled;
                        existingRecord.Source = item.Source;
                    }
                    else
                    {
                        _javaEntrys.Add(item.Installation.JavaExePath, item);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "无法读取 Java 配置项");
        }
    }

    public async Task ScanJavaAsync(bool force = false)
    {
        if (ShouldSkip()) return;

        if (!await _scanLock.WaitAsync(TimeSpan.FromSeconds(7))) return;
        try
        {
            if (ShouldSkip()) return;

            await Task.Run(_ScanInternal);
            _lastScanTime = DateTime.Now;
            SaveConfig();
        }
        finally
        {
            _scanLock.Release();
        }

        bool ShouldSkip()
        {
            return !force && (DateTime.Now - _lastScanTime) < _MinScanInterval;
        }
    }

    private void _ScanInternal()
    {
        var pathSet = new ConcurrentDictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

        Parallel.ForEach(_scanners, scanner =>
        {
            var temp = new List<string>();
            scanner.Scan(temp);
            foreach (var path in temp)
            {
                var normalized = _NormalizePath(path);
                if (!_ShouldExcludePath(normalized))
                    pathSet.TryAdd(normalized, true);
            }
        });

        var scannedEntries = pathSet.Keys
            .Select(_parser.Parse)
            .Where(inst => inst != null)
            .Select(inst => new JavaEntry
            {
                Installation = inst!,
                IsEnabled = _javaEntrys.TryGetValue(_NormalizePath(inst!.JavaExePath), out var existingJava)
                    ? existingJava.IsEnabled
                    : _ShouldEnableByDefault(inst!),
                Source = JavaSource.AutoScanned
            })
            .ToList();

        lock (_javaEntrys)
        {
            foreach (var entry in scannedEntries)
            {
                _javaEntrys[entry.Installation.JavaExePath] = entry;
            }
        }
    }

    private static bool _ShouldEnableByDefault(JavaInstallation inst)
    {
        var libDir = Path.Combine(Directory.GetParent(inst.JavaFolder)!.FullName, "lib");
        var isUsable = (!inst.IsJre && File.Exists(Path.Combine(libDir, "jvm.lib"))) ||
                       (inst.IsJre && File.Exists(Path.Combine(libDir, "rt.jar")));

        return !((inst.IsJre && inst.MajorVersion > 8) ||
                 (inst.Is64Bit ^ Environment.Is64BitOperatingSystem) ||
                 !isUsable);
    }

    public List<JavaEntry> GetSortedJavaList()
    {
        var ret = _javaEntrys.Values.ToList();
        ret.Sort((a, b) =>
        {
            var versionCmp = a.Installation.Version.CompareTo(b.Installation.Version);
            if (versionCmp != 0) return versionCmp;
            return a.Installation.Brand - b.Installation.Brand;
        });
        ret.Reverse();
        return ret;
    }

    public bool Existing64BitJava()
    {
        lock (_javaEntrys)
        {
            return _javaEntrys.Any(x => x.Value.Installation.Is64Bit);
        }
    }

    public bool ExistAnyJava()
    {
        return _javaEntrys.Count != 0;
    }

    public bool Exist(string javaExePath)
    {
        return _javaEntrys.ContainsKey(javaExePath);
    }

    public JavaEntry? AddOrGet(string javaExePath)
    {
        try
        {
            if (javaExePath.IsNullOrWhiteSpace() || !File.Exists(javaExePath)) return null;

            var installation = _parser.Parse(javaExePath);
            if (installation == null) return null;

            var exePath = _NormalizePath(installation.JavaExePath);
            lock (_javaEntrys)
            {
                if (_javaEntrys.TryGetValue(exePath, out var ret))
                    return ret;

                var entry = new JavaEntry
                {
                    Installation = installation,
                    IsEnabled = _ShouldEnableByDefault(installation),
                    Source = JavaSource.ManualAdded
                };

                _javaEntrys.Add(exePath, entry);
                return entry;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "添加或获取 {JavaExePath} 失败", javaExePath);
            return null;
        }
    }

    public JavaEntry? Get(string javaExePath)
    {
        try
        {
            if (javaExePath.IsNullOrWhiteSpace() || !File.Exists(javaExePath)) return null;

            var installation = _parser.Parse(javaExePath);
            if (installation == null) return null;

            var exePath = _NormalizePath(installation.JavaExePath);
            lock (_javaEntrys)
            {
                if (_javaEntrys.TryGetValue(exePath, out var ret))
                    return ret;

                var entry = new JavaEntry
                {
                    Installation = installation,
                    IsEnabled = _ShouldEnableByDefault(installation),
                    Source = JavaSource.ManualAdded
                };

                return entry;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取 {JavaExePath} 失败", javaExePath);
            return null;
        }
    }

    public async Task<JavaEntry[]> SelectSuitableJavaAsync(Version minVersion, Version maxVersion)
    {
        if (_javaEntrys.Count == 0)
            await ScanJavaAsync();

        lock (_javaEntrys)
        {
            return _javaEntrys
                .Values.ToList()
                .Where(j => j.Installation.IsStillAvailable && j.IsEnabled &&
                            IsVersionSuitable(j.Installation.Version, minVersion, maxVersion))
                .OrderBy(static j => j.Installation.MajorVersion)
                .ThenBy(static j => j.Installation.IsJre)
                .ThenBy(static j => j.Installation.Brand)
                .ThenByDescending(static j => j.Installation.Version)
                .ToArray();
        }
    }

    public void CheckAllAvailability()
    {
        lock (_javaEntrys)
        {
            var keys4Remove = _javaEntrys
                .Where(kv => !kv.Value.Installation.IsStillAvailable)
                .Select(kv => kv.Key)
                .ToArray();
            foreach (var key in keys4Remove)
                _javaEntrys.Remove(key);
        }
    }

    private static string _NormalizePath(string path) =>
        Path.GetFullPath(path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar))
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

    private static bool _ShouldExcludePath(string path) =>
        path.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            .Any(part => JavaConsts.ExcludeFolderNames.Contains(part, StringComparer.OrdinalIgnoreCase));

    public static Version NormalizeVersion(Version version) =>
        version.Major == 1 && version.Minor >= 0
            ? new Version(version.Minor, Math.Max(version.Build, 0), Math.Max(version.Revision, 0))
            : version;

    public static bool IsVersionSuitable(Version javaVersion, Version minVersion, Version maxVersion)
    {
        var normalizedJava = NormalizeVersion(javaVersion);
        var normalizedMin = NormalizeVersion(minVersion);
        var normalizedMax = NormalizeVersion(maxVersion);

        return normalizedJava >= normalizedMin && normalizedJava <= normalizedMax;
    }
}