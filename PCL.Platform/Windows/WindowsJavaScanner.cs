using Microsoft.Win32;
using PCL.Core.Abstractions;
using PCL.Core.Logging;
using System.IO;

namespace PCL.Platform.Windows;

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
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, "Java", "注册表扫描失败");
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
            LogWrapper.Error(ex, "Java", $"目录扫描失败: {directory}");
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
            using var regKey = Registry.LocalMachine.OpenSubKey(regPath);
            if (regKey == null) continue;

            foreach (var subKeyName in regKey.GetSubKeyNames())
            {
                using var subKey = regKey.OpenSubKey(subKeyName);
                var javaHome = subKey?.GetValue("JavaHome") as string;
                if (string.IsNullOrEmpty(javaHome) ||
                    Path.GetInvalidPathChars().Any(c => javaHome.Contains(c))) continue;

                var javaExePath = Path.Combine(javaHome, "bin", "java.exe");
                if (File.Exists(javaExePath)) results.Add(javaExePath);
            }
        }
    }

    private void ScanBrandRegistry(List<string> results)
    {
        foreach (var keyPath in BrandRegistryPaths)
        {
            using var brandKey = Registry.LocalMachine.OpenSubKey(keyPath);
            if (brandKey == null) continue;

            foreach (var subKeyName in brandKey.GetSubKeyNames())
            {
                using var subKey = brandKey.OpenSubKey(subKeyName);
                var installPath = subKey?.GetValue("InstallationPath") as string;
                if (string.IsNullOrEmpty(installPath) ||
                    Path.GetInvalidPathChars().Any(c => installPath.Contains(c))) continue;

                var javaExePath = Path.Combine(installPath, "bin", "java.exe");
                if (File.Exists(javaExePath)) results.Add(javaExePath);
            }
        }
    }
}
