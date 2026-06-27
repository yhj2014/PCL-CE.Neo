using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace PCL_CE.Neo.Core.Minecraft;

/// <summary>
/// Java品牌类型
/// </summary>
public enum JavaBrandType
{
    Unknown,
    Oracle,
    AdoptOpenJDK,
    EclipseTemurin,
    AmazonCorretto,
    Microsoft,
    AzulZulu,
    OpenJ9,
    GraalVM,
    TencentKona,
    AlibabaDragonwell,
    IBMSemeru,
    BellSoftLiberica,
    JetBrains,
    OpenJDK
}

/// <summary>
/// 机器架构类型
/// </summary>
public enum MachineType
{
    Unknown,
    X86,
    X64,
    ARM64
}

/// <summary>
/// Java安装信息
/// </summary>
public sealed record JavaInstallation(
    string JavaFolder,
    Version Version,
    JavaBrandType Brand,
    MachineType Architecture,
    bool Is64Bit,
    bool IsJre
)
{
    /// <summary>
    /// Java可执行文件路径
    /// </summary>
    public string JavaExePath => GetExecutablePath("java");
    
    /// <summary>
    /// Javaw可执行文件路径
    /// </summary>
    public string? JavawExePath => GetExecutablePath("javaw");
    
    private string GetExecutablePath(string name)
    {
        var ext = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ".exe" : "";
        return Path.Combine(JavaFolder, "bin", $"{name}{ext}");
    }

    /// <summary>
    /// Java主版本号（处理 1.8 → 8 的映射）
    /// </summary>
    public int MajorVersion => Version.Major == 1 ? Version.Minor : Version.Major;

    /// <summary>
    /// 检查物理文件是否存在
    /// </summary>
    public bool IsStillAvailable => File.Exists(JavaExePath);

    public override string ToString() =>
        $"{(IsJre ? "JRE" : "JDK")} {MajorVersion} {Brand} {(Is64Bit ? "64 Bit" : "32 Bit")} | {JavaFolder}";

    public string ToDetailedString() =>
        $"{(IsJre ? "JRE" : "JDK")} {Version} {Brand} {(Is64Bit ? "64 Bit" : "32 Bit")} | {JavaFolder}";
}

/// <summary>
/// Java入口（包含启用状态和来源）
/// </summary>
public sealed class JavaEntry
{
    public required JavaInstallation Installation { get; init; }
    public bool IsEnabled { get; set; } = true;
    public JavaSource Source { get; set; } = JavaSource.AutoScanned;

    public override string ToString() =>
        $"{(IsEnabled ? "[✓]" : "[ ]")} {Installation}";
}

/// <summary>
/// Java来源
/// </summary>
public enum JavaSource
{
    AutoScanned,
    UserAdded,
    Downloaded,
    Inherited
}

/// <summary>
/// Java扫描器接口
/// </summary>
public interface IJavaScanner
{
    /// <summary>
    /// 扫描Java安装路径
    /// </summary>
    IEnumerable<string> ScanJavaPaths();
    
    /// <summary>
    /// 检查路径是否为有效的Java安装
    /// </summary>
    bool IsValidJavaPath(string path);
    
    /// <summary>
    /// 解析Java安装信息
    /// </summary>
    JavaInstallation? ParseJavaInfo(string javaPath);
}

/// <summary>
/// Java服务接口
/// </summary>
public interface IJavaService
{
    /// <summary>
    /// 获取所有已安装的Java
    /// </summary>
    Task<IReadOnlyList<JavaEntry>> GetInstalledJavaAsync();
    
    /// <summary>
    /// 根据版本获取Java
    /// </summary>
    Task<JavaEntry?> GetJavaByVersionAsync(int majorVersion);
    
    /// <summary>
    /// 为指定游戏版本查找合适的Java
    /// </summary>
    Task<JavaEntry?> FindJavaForGameVersionAsync(string gameVersion);
    
    /// <summary>
    /// 刷新Java列表
    /// </summary>
    Task RefreshAsync();
    
    /// <summary>
    /// 添加自定义Java路径
    /// </summary>
    void AddCustomJava(string path);
    
    /// <summary>
    /// 移除Java
    /// </summary>
    void RemoveJava(string javaPath);
}

/// <summary>
/// Java服务实现
/// </summary>
public sealed class JavaService : IJavaService
{
    private readonly ILogger<JavaService> _logger;
    private readonly List<JavaEntry> _javaEntries = new();
    private readonly SemaphoreSlim _scanLock = new(1, 1);
    private bool _initialized = false;

    public JavaService(ILogger<JavaService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// 获取所有已安装的Java
    /// </summary>
    public async Task<IReadOnlyList<JavaEntry>> GetInstalledJavaAsync()
    {
        if (!_initialized)
            await RefreshAsync();
        
        return _javaEntries.Where(e => e.Installation.IsStillAvailable).ToList();
    }

    /// <summary>
    /// 根据版本获取Java
    /// </summary>
    public async Task<JavaEntry?> GetJavaByVersionAsync(int majorVersion)
    {
        var javaList = await GetInstalledJavaAsync();
        return javaList
            .Where(e => e.IsEnabled && e.Installation.MajorVersion == majorVersion)
            .OrderByDescending(e => e.Installation.Is64Bit)
            .FirstOrDefault();
    }

    /// <summary>
    /// 为指定游戏版本查找合适的Java
    /// </summary>
    public async Task<JavaEntry?> FindJavaForGameVersionAsync(string gameVersion)
    {
        var requiredVersion = GetRequiredJavaVersion(gameVersion);
        var javaList = await GetInstalledJavaAsync();
        
        // 尝试找到精确匹配的版本
        var exactMatch = javaList
            .Where(e => e.IsEnabled && e.Installation.MajorVersion == requiredVersion && e.Installation.Is64Bit)
            .FirstOrDefault();
        
        if (exactMatch != null)
            return exactMatch;
        
        // 如果没有精确匹配，尝试找到更高版本的64位Java
        var higherVersion = javaList
            .Where(e => e.IsEnabled && e.Installation.MajorVersion >= requiredVersion && e.Installation.Is64Bit)
            .OrderBy(e => e.Installation.MajorVersion)
            .FirstOrDefault();
        
        return higherVersion;
    }

    /// <summary>
    /// 刷新Java列表
    /// </summary>
    public async Task RefreshAsync()
    {
        await _scanLock.WaitAsync();
        try
        {
            _javaEntries.Clear();
            
            _logger.LogInformation("开始扫描Java安装...");
            
            // 扫描系统Java
            var systemJavaPaths = ScanSystemJava();
            foreach (var path in systemJavaPaths)
            {
                try
                {
                    var installation = ParseJavaInstallation(path);
                    if (installation != null)
                    {
                        _javaEntries.Add(new JavaEntry
                        {
                            Installation = installation,
                            IsEnabled = true,
                            Source = JavaSource.AutoScanned
                        });
                        
                        _logger.LogDebug("发现Java: {Installation}", installation);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "解析Java路径失败: {Path}", path);
                }
            }
            
            _logger.LogInformation("Java扫描完成，共发现 {Count} 个安装", _javaEntries.Count);
            _initialized = true;
        }
        finally
        {
            _scanLock.Release();
        }
    }

    /// <summary>
    /// 添加自定义Java路径
    /// </summary>
    public void AddCustomJava(string path)
    {
        try
        {
            var installation = ParseJavaInstallation(path);
            if (installation != null)
            {
                var entry = new JavaEntry
                {
                    Installation = installation,
                    IsEnabled = true,
                    Source = JavaSource.UserAdded
                };
                
                // 检查是否已存在
                if (!_javaEntries.Any(e => e.Installation.JavaFolder == installation.JavaFolder))
                {
                    _javaEntries.Add(entry);
                    _logger.LogInformation("添加自定义Java: {Installation}", installation);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "添加自定义Java失败: {Path}", path);
        }
    }

    /// <summary>
    /// 移除Java
    /// </summary>
    public void RemoveJava(string javaPath)
    {
        var entry = _javaEntries.FirstOrDefault(e => e.Installation.JavaFolder == javaPath);
        if (entry != null)
        {
            _javaEntries.Remove(entry);
            _logger.LogInformation("移除Java: {Path}", javaPath);
        }
    }

    /// <summary>
    /// 扫描系统中的Java安装
    /// </summary>
    private IEnumerable<string> ScanSystemJava()
    {
        var paths = new HashSet<string>();
        
        // Windows平台
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            // 1. 从注册表扫描
            paths.UnionWith(ScanFromRegistry());
            
            // 2. 从环境变量扫描
            paths.UnionWith(ScanFromEnvironment());
            
            // 3. 从默认路径扫描
            paths.UnionWith(ScanDefaultPathsWindows());
            
            // 4. 从 Microsoft Store 扫描
            paths.UnionWith(ScanMicrosoftStore());
        }
        // macOS平台
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            paths.UnionWith(ScanMacOS());
        }
        // Linux平台
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            paths.UnionWith(ScanLinux());
        }
        
        return paths.Where(p => IsValidJavaPath(p));
    }

    /// <summary>
    /// 从Windows注册表扫描Java
    /// </summary>
    private IEnumerable<string> ScanFromRegistry()
    {
        var paths = new List<string>();
        
        try
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return paths;
            
            // 检查 JDK 注册表项
            var jdkKeys = new[]
            {
                "SOFTWARE\\JavaSoft\\JDK",
                "SOFTWARE\\JavaSoft\\Java Development Kit",
                "SOFTWARE\\Eclipse Adoptium\\JDK",
                "SOFTWARE\\Azul Systems\\Zulu",
                "SOFTWARE\\Amazon\\Corretto"
            };
            
            foreach (var keyPath in jdkKeys)
            {
                try
                {
                    // 使用 Process 启动 reg query 命令
                    using var process = Process.Start(new ProcessStartInfo
                    {
                        FileName = "reg",
                        Arguments = $"query \"{keyPath}\" /s",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    });
                    
                    if (process != null)
                    {
                        var output = process.StandardOutput.ReadToEnd();
                        var matches = Regex.Matches(output, "JavaHome\\s+REG_SZ\\s+(.+)");
                        
                        foreach (Match match in matches)
                        {
                            var javaHome = match.Groups[1].Value.Trim();
                            if (!string.IsNullOrEmpty(javaHome) && Directory.Exists(javaHome))
                            {
                                paths.Add(javaHome);
                            }
                        }
                    }
                }
                catch
                {
                    // 忽略注册表访问错误
                }
            }
            
            // 检查 JRE 注册表项
            var jreKeys = new[]
            {
                "SOFTWARE\\JavaSoft\\JRE",
                "SOFTWARE\\JavaSoft\\Java Runtime Environment"
            };
            
            foreach (var keyPath in jreKeys)
            {
                try
                {
                    using var process = Process.Start(new ProcessStartInfo
                    {
                        FileName = "reg",
                        Arguments = $"query \"{keyPath}\" /s",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    });
                    
                    if (process != null)
                    {
                        var output = process.StandardOutput.ReadToEnd();
                        var matches = Regex.Matches(output, "JavaHome\\s+REG_SZ\\s+(.+)");
                        
                        foreach (Match match in matches)
                        {
                            var javaHome = match.Groups[1].Value.Trim();
                            if (!string.IsNullOrEmpty(javaHome) && Directory.Exists(javaHome))
                            {
                                paths.Add(javaHome);
                            }
                        }
                    }
                }
                catch
                {
                    // 忽略注册表访问错误
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "从注册表扫描Java失败");
        }
        
        return paths;
    }

    /// <summary>
    /// 从环境变量扫描Java
    /// </summary>
    private IEnumerable<string> ScanFromEnvironment()
    {
        var paths = new List<string>();
        
        // JAVA_HOME 环境变量
        var javaHome = Environment.GetEnvironmentVariable("JAVA_HOME");
        if (!string.IsNullOrEmpty(javaHome) && Directory.Exists(javaHome))
        {
            paths.Add(javaHome);
        }
        
        // PATH 环境变量中的 Java
        var pathEnv = Environment.GetEnvironmentVariable("PATH");
        if (!string.IsNullOrEmpty(pathEnv))
        {
            var pathParts = pathEnv.Split(Path.PathSeparator);
            foreach (var part in pathParts)
            {
                var trimmed = part.Trim();
                if (trimmed.Contains("java") || trimmed.Contains("jdk") || trimmed.Contains("jre"))
                {
                    // 找到 bin 目录，取父目录
                    var parent = Path.GetDirectoryName(trimmed);
                    if (!string.IsNullOrEmpty(parent) && Directory.Exists(parent))
                    {
                        paths.Add(parent);
                    }
                }
            }
        }
        
        return paths;
    }

    /// <summary>
    /// 扫描Windows默认路径
    /// </summary>
    private IEnumerable<string> ScanDefaultPathsWindows()
    {
        var defaultPaths = new[]
        {
            "C:\\Program Files\\Java",
            "C:\\Program Files (x86)\\Java",
            "C:\\Program Files\\Eclipse Adoptium",
            "C:\\Program Files\\AdoptOpenJDK",
            "C:\\Program Files\\Amazon\\Corretto",
            "C:\\Program Files\\Zulu",
            "C:\\Program Files\\Microsoft",
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", "Eclipse Adoptium"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", "AdoptOpenJDK")
        };
        
        var foundPaths = new List<string>();
        
        foreach (var basePath in defaultPaths)
        {
            if (!Directory.Exists(basePath))
                continue;
            
            try
            {
                foreach (var dir in Directory.GetDirectories(basePath))
                {
                    var name = Path.GetFileName(dir).ToLower();
                    if (name.Contains("jdk") || name.Contains("jre"))
                    {
                        foundPaths.Add(dir);
                    }
                }
            }
            catch
            {
                // 忽略目录访问错误
            }
        }
        
        return foundPaths;
    }

    /// <summary>
    /// 扫描Microsoft Store安装的Java
    /// </summary>
    private IEnumerable<string> ScanMicrosoftStore()
    {
        var paths = new List<string>();
        
        try
        {
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var msPath = Path.Combine(localAppData, "Microsoft", "WindowsApps");
            
            if (Directory.Exists(msPath))
            {
                // 检查是否有 java.exe
                var javaExe = Path.Combine(msPath, "java.exe");
                if (File.Exists(javaExe))
                {
                    // Microsoft Store Java 的路径处理比较特殊
                    paths.Add(msPath);
                }
            }
        }
        catch
        {
            // 忽略错误
        }
        
        return paths;
    }

    /// <summary>
    /// 扫描macOS Java
    /// </summary>
    private IEnumerable<string> ScanMacOS()
    {
        var paths = new List<string>();
        
        // /Library/Java/JavaVirtualMachines
        var jvmBase = "/Library/Java/JavaVirtualMachines";
        if (Directory.Exists(jvmBase))
        {
            try
            {
                foreach (var dir in Directory.GetDirectories(jvmBase))
                {
                    var contentsHome = Path.Combine(dir, "Contents", "Home");
                    if (Directory.Exists(contentsHome))
                    {
                        paths.Add(contentsHome);
                    }
                }
            }
            catch
            {
                // 忽略错误
            }
        }
        
        // 用户目录
        var userJvm = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".sdkman", "candidates", "java");
        if (Directory.Exists(userJvm))
        {
            try
            {
                foreach (var dir in Directory.GetDirectories(userJvm))
                {
                    paths.Add(dir);
                }
            }
            catch
            {
                // 忽略错误
            }
        }
        
        // 使用 /usr/libexec/java_home 命令
        try
        {
            using var process = Process.Start(new ProcessStartInfo
            {
                FileName = "/usr/libexec/java_home",
                Arguments = "-V",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            });
            
            if (process != null)
            {
                var output = process.StandardOutput.ReadToEnd();
                var error = process.StandardError.ReadToEnd();
                
                // 解析输出
                var lines = (output + error).Split('\n');
                foreach (var line in lines)
                {
                    var match = Regex.Match(line, @"(\d+,\d+,\d+,\d+).*from:\s+(.+)");
                    if (match.Success)
                    {
                        var javaPath = match.Groups[2].Value.Trim();
                        if (Directory.Exists(javaPath))
                        {
                            paths.Add(javaPath);
                        }
                    }
                }
            }
        }
        catch
        {
            // 忽略错误
        }
        
        return paths;
    }

    /// <summary>
    /// 扫描Linux Java
    /// </summary>
    private IEnumerable<string> ScanLinux()
    {
        var paths = new List<string>();
        
        var searchPaths = new[]
        {
            "/usr/lib/jvm",
            "/usr/java",
            "/opt/java",
            "/opt/jdk",
            "/opt/jre",
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".sdkman", "candidates", "java"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Java")
        };
        
        foreach (var basePath in searchPaths)
        {
            if (!Directory.Exists(basePath))
                continue;
            
            try
            {
                foreach (var dir in Directory.GetDirectories(basePath))
                {
                    var name = Path.GetFileName(dir).ToLower();
                    if (name.Contains("jdk") || name.Contains("jre") || name.Contains("java"))
                    {
                        paths.Add(dir);
                    }
                }
            }
            catch
            {
                // 忽略错误
            }
        }
        
        // 使用 whereis 或 which 命令
        try
        {
            using var process = Process.Start(new ProcessStartInfo
            {
                FileName = "which",
                Arguments = "java",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            });
            
            if (process != null)
            {
                var output = process.StandardOutput.ReadToEnd().Trim();
                if (!string.IsNullOrEmpty(output))
                {
                    // java 通常在 bin 目录，取父目录的父目录
                    var binDir = Path.GetDirectoryName(output);
                    if (!string.IsNullOrEmpty(binDir))
                    {
                        var javaHome = Path.GetDirectoryName(binDir);
                        if (!string.IsNullOrEmpty(javaHome))
                        {
                            paths.Add(javaHome);
                        }
                    }
                }
            }
        }
        catch
        {
            // 忽略错误
        }
        
        return paths;
    }

    /// <summary>
    /// 检查路径是否为有效的Java安装
    /// </summary>
    private bool IsValidJavaPath(string path)
    {
        if (!Directory.Exists(path))
            return false;
        
        var ext = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ".exe" : "";
        var javaExe = Path.Combine(path, "bin", $"java{ext}");
        
        return File.Exists(javaExe);
    }

    /// <summary>
    /// 解析Java安装信息
    /// </summary>
    private JavaInstallation? ParseJavaInstallation(string javaPath)
    {
        try
        {
            var ext = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ".exe" : "";
            var javaExe = Path.Combine(javaPath, "bin", $"java{ext}");
            
            if (!File.Exists(javaExe))
                return null;
            
            // 运行 java -version 获取版本信息
            using var process = Process.Start(new ProcessStartInfo
            {
                FileName = javaExe,
                Arguments = "-version",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            });
            
            if (process == null)
                return null;
            
            process.WaitForExit(5000);
            var output = process.StandardError.ReadToEnd() + process.StandardOutput.ReadToEnd();
            
            // 解析版本号
            var version = ParseJavaVersion(output);
            
            // 解析品牌
            var brand = ParseJavaBrand(output, javaPath);
            
            // 解析架构
            var (arch, is64Bit) = ParseJavaArchitecture(output);
            
            // 检查是否为JRE
            var isJre = CheckIsJre(javaPath);
            
            return new JavaInstallation(
                JavaFolder: javaPath,
                Version: version,
                Brand: brand,
                Architecture: arch,
                Is64Bit: is64Bit,
                IsJre: isJre
            );
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "解析Java安装失败: {Path}", javaPath);
            return null;
        }
    }

    /// <summary>
    /// 解析Java版本号
    /// </summary>
    private Version ParseJavaVersion(string versionOutput)
    {
        // 匹配 "version "1.8.0_301"" 或 "version "11.0.12"" 等
        var match = Regex.Match(versionOutput, @"version\s+\"([^\"]+)\"");
        if (match.Success)
        {
            var versionStr = match.Groups[1].Value;
            
            // 处理 1.8.0_301 格式
            if (versionStr.StartsWith("1."))
            {
                var parts = versionStr.Split('_', '.');
                if (parts.Length >= 3)
                {
                    var major = int.Parse(parts[0]); // 1
                    var minor = int.Parse(parts[1]); // 8
                    var build = parts.Length > 3 ? int.Parse(parts[3]) : 0;
                    return new Version(major, minor, build);
                }
            }
            
            // 处理 11.0.12 格式
            var versionParts = versionStr.Split('.');
            if (versionParts.Length >= 1)
            {
                var major = int.Parse(versionParts[0]);
                var minor = versionParts.Length > 1 ? int.Parse(versionParts[1]) : 0;
                var build = versionParts.Length > 2 ? int.Parse(versionParts[2].Split('_')[0]) : 0;
                return new Version(major, minor, build);
            }
        }
        
        return new Version(0, 0);
    }

    /// <summary>
    /// 解析Java品牌
    /// </summary>
    private JavaBrandType ParseJavaBrand(string versionOutput, string javaPath)
    {
        var outputLower = versionOutput.ToLower();
        var pathLower = javaPath.ToLower();
        
        if (outputLower.Contains("temurin") || pathLower.Contains("temurin") || pathLower.Contains("adoptium"))
            return JavaBrandType.EclipseTemurin;
        
        if (outputLower.Contains("openj9") || pathLower.Contains("openj9") || pathLower.Contains("semeru"))
            return JavaBrandType.OpenJ9;
        
        if (outputLower.Contains("corretto") || pathLower.Contains("corretto"))
            return JavaBrandType.AmazonCorretto;
        
        if (outputLower.Contains("zulu") || pathLower.Contains("zulu"))
            return JavaBrandType.AzulZulu;
        
        if (outputLower.Contains("graalvm") || pathLower.Contains("graalvm"))
            return JavaBrandType.GraalVM;
        
        if (outputLower.Contains("kona") || pathLower.Contains("kona"))
            return JavaBrandType.TencentKona;
        
        if (outputLower.Contains("dragonwell") || pathLower.Contains("dragonwell"))
            return JavaBrandType.AlibabaDragonwell;
        
        if (outputLower.Contains("liberica") || pathLower.Contains("liberica"))
            return JavaBrandType.BellSoftLiberica;
        
        if (outputLower.Contains("jetbrains") || pathLower.Contains("jetbrains"))
            return JavaBrandType.JetBrains;
        
        if (outputLower.Contains("microsoft") || pathLower.Contains("microsoft"))
            return JavaBrandType.Microsoft;
        
        if (outputLower.Contains("adoptopenjdk") || pathLower.Contains("adoptopenjdk"))
            return JavaBrandType.AdoptOpenJDK;
        
        if (outputLower.Contains("oracle"))
            return JavaBrandType.Oracle;
        
        return JavaBrandType.OpenJDK;
    }

    /// <summary>
    /// 解析Java架构
    /// </summary>
    private (MachineType, bool) ParseJavaArchitecture(string versionOutput)
    {
        var outputLower = versionOutput.ToLower();
        
        if (outputLower.Contains("64-bit") || outputLower.Contains("x86_64") || outputLower.Contains("amd64"))
        {
            return (MachineType.X64, true);
        }
        
        if (outputLower.Contains("arm64") || outputLower.Contains("aarch64"))
        {
            return (MachineType.ARM64, true);
        }
        
        if (outputLower.Contains("32-bit") || outputLower.Contains("x86"))
        {
            return (MachineType.X86, false);
        }
        
        // 默认根据当前系统判断
        var is64Bit = RuntimeInformation.ProcessArchitecture == Architecture.X64 ||
                      RuntimeInformation.ProcessArchitecture == Architecture.Arm64;
        
        return (MachineType.Unknown, is64Bit);
    }

    /// <summary>
    /// 检查是否为JRE
    /// </summary>
    private bool CheckIsJre(string javaPath)
    {
        // 检查是否有 javac.exe/javac（JDK特有）
        var ext = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ".exe" : "";
        var javacPath = Path.Combine(javaPath, "bin", $"javac{ext}");
        
        return !File.Exists(javacPath);
    }

    /// <summary>
    /// 获取游戏版本所需的Java版本
    /// </summary>
    private int GetRequiredJavaVersion(string gameVersion)
    {
        // 1.7.x - 1.16.5: Java 8
        // 1.17.x - 1.17.1: Java 16
        // 1.18+: Java 17
        // 1.20.5+: Java 21
        
        if (gameVersion.StartsWith("1.7") || gameVersion.StartsWith("1.8") ||
            gameVersion.StartsWith("1.9") || gameVersion.StartsWith("1.10") ||
            gameVersion.StartsWith("1.11") || gameVersion.StartsWith("1.12") ||
            gameVersion.StartsWith("1.13") || gameVersion.StartsWith("1.14") ||
            gameVersion.StartsWith("1.15") || gameVersion.StartsWith("1.16"))
        {
            return 8;
        }
        
        if (gameVersion.StartsWith("1.17"))
        {
            return 16;
        }
        
        if (gameVersion.StartsWith("1.18") || gameVersion.StartsWith("1.19") ||
            gameVersion.StartsWith("1.20"))
        {
            // 1.20.5+ 需要 Java 21
            if (gameVersion.StartsWith("1.20"))
            {
                var parts = gameVersion.Split('.');
                if (parts.Length >= 3 && int.TryParse(parts[2], out var minor))
                {
                    if (minor >= 5)
                        return 21;
                }
            }
            
            return 17;
        }
        
        // 默认 Java 21
        return 21;
    }
}