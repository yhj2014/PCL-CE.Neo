using System.Collections.Generic;
using System.IO;

namespace PCL_CE.Neo.Core.Minecraft.Java.Scanner;

public class DefaultPathsScanner : IJavaScanner
{
    public void Scan(ICollection<string> results)
    {
        if (OperatingSystem.IsWindows())
        {
            _ScanWindowsPaths(results);
        }
        else if (OperatingSystem.IsLinux())
        {
            _ScanLinuxPaths(results);
        }
        else if (OperatingSystem.IsMacOS())
        {
            _ScanMacPaths(results);
        }
    }

    private static void _ScanWindowsPaths(ICollection<string> results)
    {
        var candidates = new[]
        {
            @"C:\Program Files\Java",
            @"C:\Program Files (x86)\Java",
            Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), "Programs", "Java"),
            @"C:\Program Files\Eclipse Adoptium",
            @"C:\Program Files\Amazon Corretto",
            @"C:\Program Files\Zulu",
            @"C:\Program Files\BellSoft"
        };

        foreach (var candidate in candidates)
        {
            if (Directory.Exists(candidate))
            {
                foreach (var dir in Directory.GetDirectories(candidate))
                {
                    var javaExe = Path.Combine(dir, "bin", "java.exe");
                    if (File.Exists(javaExe))
                        results.Add(javaExe);
                }
            }
        }
    }

    private static void _ScanLinuxPaths(ICollection<string> results)
    {
        var candidates = new[]
        {
            "/usr/lib/jvm",
            "/usr/java",
            "/opt/java",
            "/usr/local/java",
            Path.Combine(System.Environment.GetEnvironmentVariable("HOME") ?? "", ".jdks")
        };

        foreach (var candidate in candidates)
        {
            if (Directory.Exists(candidate))
            {
                foreach (var dir in Directory.GetDirectories(candidate))
                {
                    var javaExe = Path.Combine(dir, "bin", "java");
                    if (File.Exists(javaExe))
                        results.Add(javaExe);
                }
            }
        }
    }

    private static void _ScanMacPaths(ICollection<string> results)
    {
        var candidates = new[]
        {
            "/Library/Java/JavaVirtualMachines",
            Path.Combine(System.Environment.GetEnvironmentVariable("HOME") ?? "", "Library/Java/JavaVirtualMachines"),
            "/usr/local/opt"
        };

        foreach (var candidate in candidates)
        {
            if (Directory.Exists(candidate))
            {
                foreach (var dir in Directory.GetDirectories(candidate))
                {
                    var javaExe = Path.Combine(dir, "Contents", "Home", "bin", "java");
                    if (File.Exists(javaExe))
                        results.Add(javaExe);
                }
            }
        }
    }
}