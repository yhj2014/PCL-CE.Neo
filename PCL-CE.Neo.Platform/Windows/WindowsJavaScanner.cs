using Microsoft.Win32;
using PCL_CE.Neo.Core.Abstractions;
using System.IO;

namespace PCL_CE.Neo.Platform.Windows;

public class WindowsJavaScanner : IJavaScanner
{
    private static readonly string[] RegistryPaths =
    [
        @"SOFTWARE\JavaSoft\Java Development Kit",
        @"SOFTWARE\JavaSoft\Java Runtime Environment",
        @"SOFTWARE\WOW6432Node\JavaSoft\Java Development Kit",
        @"SOFTWARE\WOW6432Node\JavaSoft\Java Runtime Environment"
    ];

    private static readonly string[] BrandRegistryPaths =
    [
        @"SOFTWARE\Azul Systems\Zulu",
        @"SOFTWARE\BellSoft\Liberica"
    ];

    public IEnumerable<string> ScanJavaPaths()
    {
        var results = new List<string>();
        try
        {
            ScanJavaSoftRegistry(results);
            ScanBrandRegistry(results);
            // 也扫描常见的 Java 安装目录
            ScanCommonDirectories(results);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Java 注册表扫描失败: {ex.Message}");
        }
        return results;
    }

    public IEnumerable<string> ScanDirectory(string directory)
    {
        var results = new List<string>();
        if (!Directory.Exists(directory)) return results;

        try
        {
            var javaExePath = Path.Combine(directory, "bin", "java.exe");
            if (File.Exists(javaExePath))
            {
                results.Add(javaExePath);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Java 目录扫描失败 ({directory}): {ex.Message}");
        }
        return results;
    }

    public bool IsValidJavaPath(string path)
    {
        if (string.IsNullOrEmpty(path)) return false;
        if (!File.Exists(path)) return false;
        return Path.GetFileName(path).Equals("java.exe", StringComparison.OrdinalIgnoreCase);
    }

    private void ScanJavaSoftRegistry(List<string> results)
    {
        foreach (var regPath in RegistryPaths)
        {
            try
            {
                using var regKey = Registry.LocalMachine.OpenSubKey(regPath);
                if (regKey == null) continue;

                foreach (var subKeyName in regKey.GetSubKeyNames())
                {
                    try
                    {
                        using var subKey = regKey.OpenSubKey(subKeyName);
                        var javaHome = subKey?.GetValue("JavaHome") as string;
                        if (string.IsNullOrEmpty(javaHome) ||
                            Path.GetInvalidPathChars().Any(c => javaHome.Contains(c))) continue;

                        var javaExePath = Path.Combine(javaHome, "bin", "java.exe");
                        if (File.Exists(javaExePath) && !results.Contains(javaExePath))
                        {
                            results.Add(javaExePath);
                        }
                    }
                    catch
                    {
                        // 忽略单个子键的错误
                    }
                }
            }
            catch
            {
                // 忽略单个注册表路径的错误
            }
        }
    }

    private void ScanBrandRegistry(List<string> results)
    {
        foreach (var keyPath in BrandRegistryPaths)
        {
            try
            {
                using var brandKey = Registry.LocalMachine.OpenSubKey(keyPath);
                if (brandKey == null) continue;

                foreach (var subKeyName in brandKey.GetSubKeyNames())
                {
                    try
                    {
                        using var subKey = brandKey.OpenSubKey(subKeyName);
                        var installPath = subKey?.GetValue("InstallationPath") as string;
                        if (string.IsNullOrEmpty(installPath) ||
                            Path.GetInvalidPathChars().Any(c => installPath.Contains(c))) continue;

                        var javaExePath = Path.Combine(installPath, "bin", "java.exe");
                        if (File.Exists(javaExePath) && !results.Contains(javaExePath))
                        {
                            results.Add(javaExePath);
                        }
                    }
                    catch
                    {
                        // 忽略单个子键的错误
                    }
                }
            }
            catch
            {
                // 忽略单个注册表路径的错误
            }
        }
    }

    private void ScanCommonDirectories(List<string> results)
    {
        // 常见的 Java 安装目录
        var commonDirectories = new[]
        {
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Java"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Java"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", "Java"),
            "C:\\Program Files\\Eclipse Adoptium",
            "C:\\Program Files\\Microsoft",
            "C:\\Program Files\\Zulu"
        };

        foreach (var directory in commonDirectories)
        {
            if (Directory.Exists(directory))
            {
                try
                {
                    // 查找子目录
                    foreach (var subDir in Directory.GetDirectories(directory))
                    {
                        var javaExePath = Path.Combine(subDir, "bin", "java.exe");
                        if (File.Exists(javaExePath) && !results.Contains(javaExePath))
                        {
                            results.Add(javaExePath);
                        }
                    }
                }
                catch
                {
                    // 忽略单个目录的错误
                }
            }
        }
    }
}
