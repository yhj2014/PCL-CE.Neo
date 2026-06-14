using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;

namespace PCL_CE.Neo.Core.Abstractions;

/// <summary>
/// 默认 Java 扫描器实现
/// </summary>
public class DefaultJavaScanner : IJavaScanner
{
    private readonly ILogger<DefaultJavaScanner>? _logger;

    public DefaultJavaScanner(ILogger<DefaultJavaScanner>? logger = null)
    {
        _logger = logger;
    }

    /// <summary>
    /// 扫描系统中的 Java 安装路径
    /// </summary>
    public IEnumerable<string> ScanJavaPaths()
    {
        var paths = new List<string>();

        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                paths.AddRange(ScanWindowsJava());
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                paths.AddRange(ScanLinuxJava());
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                paths.AddRange(ScanMacJava());
            }

            // 扫描 PATH 环境变量
            paths.AddRange(ScanPathEnvironment());
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "扫描 Java 路径时发生异常");
        }

        return paths;
    }

    /// <summary>
    /// 扫描指定目录中的 Java 安装
    /// </summary>
    public IEnumerable<string> ScanDirectory(string directory)
    {
        var paths = new List<string>();

        if (!Directory.Exists(directory))
        {
            _logger?.LogWarning("目录不存在: {Directory}", directory);
            return paths;
        }

        try
        {
            // 查找 java 可执行文件
            var javaExe = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "java.exe" : "java";

            var files = Directory.GetFiles(directory, javaExe, SearchOption.AllDirectories);
            foreach (var file in files)
            {
                if (IsValidJavaPath(file))
                {
                    paths.Add(file);
                }
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "扫描目录时发生异常: {Directory}", directory);
        }

        return paths;
    }

    /// <summary>
    /// 验证路径是否为有效的 Java 安装
    /// </summary>
    public bool IsValidJavaPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return false;
        }

        try
        {
            if (!File.Exists(path))
            {
                return false;
            }

            // 检查文件名是否为 java 或 java.exe
            var fileName = Path.GetFileName(path);
            var expectedName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "java.exe" : "java";

            if (!string.Equals(fileName, expectedName, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            // 尝试执行 java -version
            using var process = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = path,
                    Arguments = "-version",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };

            process.Start();
            process.WaitForExit(5000);

            // Java 通常将版本信息输出到 stderr
            var output = process.StandardError.ReadToEnd();
            return output.Contains("version") || output.Contains("openjdk") || output.Contains("java");
        }
        catch (Exception ex)
        {
            _logger?.LogDebug(ex, "验证 Java 路径失败: {Path}", path);
            return false;
        }
    }

    private IEnumerable<string> ScanWindowsJava()
    {
        var paths = new List<string>();

        try
        {
            // 扫描 Program Files
            var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            var programFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);

            var searchDirs = new[]
            {
                Path.Combine(programFiles, "Java"),
                Path.Combine(programFilesX86, "Java"),
                Path.Combine(programFiles, "Eclipse Adoptium"),
                Path.Combine(programFilesX86, "Eclipse Adoptium"),
                Path.Combine(programFiles, "AdoptOpenJDK"),
                Path.Combine(programFilesX86, "AdoptOpenJDK"),
                Path.Combine(programFiles, "Microsoft"),
                Path.Combine(programFilesX86, "Microsoft"),
                Path.Combine(programFiles, "Zulu"),
                Path.Combine(programFilesX86, "Zulu")
            };

            foreach (var dir in searchDirs)
            {
                if (Directory.Exists(dir))
                {
                    paths.AddRange(ScanDirectory(dir));
                }
            }

            // 扫描注册表
            paths.AddRange(ScanWindowsRegistry());
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "扫描 Windows Java 时发生异常");
        }

        return paths;
    }

    private IEnumerable<string> ScanWindowsRegistry()
    {
        var paths = new List<string>();

        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return paths;
        }

        try
        {
            // 使用 where 命令查找 Java
            using var whereProcess = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "where",
                    Arguments = "java",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };

            whereProcess.Start();
            var output = whereProcess.StandardOutput.ReadToEnd();
            whereProcess.WaitForExit(5000);

            foreach (var line in output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var trimmed = line.Trim();
                if (IsValidJavaPath(trimmed))
                {
                    paths.Add(trimmed);
                }
            }
        }
        catch (Exception ex)
        {
            _logger?.LogDebug(ex, "使用 where 命令查找 Java 失败");
        }

        return paths;
    }

    private IEnumerable<string> ScanLinuxJava()
    {
        var paths = new List<string>();

        try
        {
            // 扫描常见 Java 安装目录
            var searchDirs = new[]
            {
                "/usr/lib/jvm",
                "/usr/java",
                "/opt/java",
                "/opt/jdk",
                "/opt/openjdk",
                "/usr/local/java"
            };

            foreach (var dir in searchDirs)
            {
                if (Directory.Exists(dir))
                {
                    paths.AddRange(ScanDirectory(dir));
                }
            }

            // 使用 which 命令
            using var whichProcess = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "which",
                    Arguments = "java",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };

            whichProcess.Start();
            var output = whichProcess.StandardOutput.ReadToEnd();
            whichProcess.WaitForExit(5000);

            foreach (var line in output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var trimmed = line.Trim();
                if (IsValidJavaPath(trimmed))
                {
                    paths.Add(trimmed);
                }
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "扫描 Linux Java 时发生异常");
        }

        return paths;
    }

    private IEnumerable<string> ScanMacJava()
    {
        var paths = new List<string>();

        try
        {
            // 扫描 macOS Java 安装目录
            var searchDirs = new[]
            {
                "/Library/Java/JavaVirtualMachines",
                "/System/Library/Java/JavaVirtualMachines",
                "/usr/local/java"
            };

            foreach (var dir in searchDirs)
            {
                if (Directory.Exists(dir))
                {
                    // macOS Java 目录结构：/Library/Java/JavaVirtualMachines/jdk-xxx.jdk/Contents/Home/bin/java
                    foreach (var jdkDir in Directory.GetDirectories(dir))
                    {
                        if (jdkDir.EndsWith(".jdk"))
                        {
                            var binDir = Path.Combine(jdkDir, "Contents", "Home", "bin");
                            if (Directory.Exists(binDir))
                            {
                                paths.AddRange(ScanDirectory(binDir));
                            }
                        }
                    }
                }
            }

            // 使用 java_home 命令
            using var javaHomeProcess = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "/usr/libexec/java_home",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };

            javaHomeProcess.Start();
            var output = javaHomeProcess.StandardOutput.ReadToEnd().Trim();
            javaHomeProcess.WaitForExit(5000);

            if (!string.IsNullOrEmpty(output) && Directory.Exists(output))
            {
                var binDir = Path.Combine(output, "bin");
                if (Directory.Exists(binDir))
                {
                    paths.AddRange(ScanDirectory(binDir));
                }
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "扫描 macOS Java 时发生异常");
        }

        return paths;
    }

    private IEnumerable<string> ScanPathEnvironment()
    {
        var paths = new List<string>();

        try
        {
            var pathEnv = Environment.GetEnvironmentVariable("PATH");
            if (string.IsNullOrEmpty(pathEnv))
            {
                return paths;
            }

            var separator = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ';' : ':';
            var pathDirs = pathEnv.Split(separator);

            foreach (var dir in pathDirs)
            {
                var trimmedDir = dir.Trim();
                if (!string.IsNullOrEmpty(trimmedDir) && Directory.Exists(trimmedDir))
                {
                    var javaExe = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "java.exe" : "java";
                    var javaPath = Path.Combine(trimmedDir, javaExe);

                    if (File.Exists(javaPath) && IsValidJavaPath(javaPath))
                    {
                        paths.Add(javaPath);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "扫描 PATH 环境变量时发生异常");
        }

        return paths;
    }
}