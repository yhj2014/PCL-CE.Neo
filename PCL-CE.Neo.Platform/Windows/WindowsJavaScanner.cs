using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace PCL_CE.Neo.Platform.Windows;

public class WindowsJavaScanner : Core.Abstractions.IJavaScanner
{
    private static readonly string[] WindowsJavaPaths = new[]
    {
        @"C:\Program Files\Java",
        @"C:\Program Files (x86)\Java",
        @"C:\Program Files\Eclipse Adoptium",
        @"C:\Program Files\Amazon Corretto",
    };

    public IEnumerable<string> ScanJavaPaths()
    {
        var javaPaths = new List<string>();

        foreach (var basePath in WindowsJavaPaths)
        {
            if (Directory.Exists(basePath))
            {
                javaPaths.AddRange(ScanDirectory(basePath));
            }
        }

        var jdkPath = Environment.GetEnvironmentVariable("JAVA_HOME");
        if (!string.IsNullOrEmpty(jdkPath) && Directory.Exists(jdkPath))
        {
            javaPaths.Add(jdkPath);
        }

        var userHome = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        if (!string.IsNullOrEmpty(userHome))
        {
            var userJdks = Path.Combine(userHome, ".jdks");
            if (Directory.Exists(userJdks))
            {
                javaPaths.AddRange(ScanDirectory(userJdks));
            }
        }

        ScanRegistryForJava(javaPaths);

        return javaPaths.Where(IsValidJavaPath).Distinct();
    }

    private void ScanRegistryForJava(List<string> javaPaths)
    {
        try
        {
            var registryPaths = new[]
            {
                @"SOFTWARE\JavaSoft\Java Runtime Environment",
                @"SOFTWARE\JavaSoft\Java Development Kit",
                @"SOFTWARE\Eclipse Adoptium\JRE",
                @"SOFTWARE\Eclipse Adoptium\JDK"
            };

            foreach (var registryPath in registryPaths)
            {
                using var key = Registry.LocalMachine.OpenSubKey(registryPath);
                if (key != null)
                {
                    foreach (var subKeyName in key.GetSubKeyNames())
                    {
                        using var subKey = key.OpenSubKey(subKeyName);
                        var javaHome = subKey?.GetValue("JavaHome") as string;
                        if (!string.IsNullOrEmpty(javaHome) && Directory.Exists(javaHome))
                        {
                            javaPaths.Add(javaHome);
                        }
                    }
                }

                using var key32 = Registry.LocalMachine.OpenSubKey(@"Wow6432Node\" + registryPath);
                if (key32 != null)
                {
                    foreach (var subKeyName in key32.GetSubKeyNames())
                    {
                        using var subKey = key32.OpenSubKey(subKeyName);
                        var javaHome = subKey?.GetValue("JavaHome") as string;
                        if (!string.IsNullOrEmpty(javaHome) && Directory.Exists(javaHome))
                        {
                            javaPaths.Add(javaHome);
                        }
                    }
                }
            }
        }
        catch { }
    }

    public IEnumerable<string> ScanDirectory(string directory)
    {
        var paths = new List<string>();

        try
        {
            if (!Directory.Exists(directory))
                return paths;

            foreach (var dir in Directory.GetDirectories(directory))
            {
                var javaExe = Path.Combine(dir, "bin", "java.exe");
                if (File.Exists(javaExe))
                {
                    paths.Add(dir);
                }
            }
        }
        catch { }

        return paths;
    }

    public bool IsValidJavaPath(string path)
    {
        if (string.IsNullOrEmpty(path))
            return false;

        try
        {
            var javaExe = Path.Combine(path, "bin", "java.exe");
            return File.Exists(javaExe);
        }
        catch
        {
            return false;
        }
    }
}
