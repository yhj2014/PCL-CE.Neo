using System;
using System.Collections.Generic;
using System.IO;

namespace PCL_CE.Neo.Core.Minecraft.Java.Scanner;

public class DefaultPathsScanner : IJavaScanner
{
    public void Scan(List<string> results)
    {
        try
        {
            var platform = Environment.OSVersion.Platform;
            
            if (platform == PlatformID.Win32NT)
            {
                _ScanWindows(results);
            }
            else if (platform == PlatformID.Unix)
            {
                _ScanUnix(results);
            }
        }
        catch
        {
        }
    }

    private void _ScanWindows(List<string> results)
    {
        var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        var programFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
        
        _ScanDirectory(results, Path.Combine(programFiles, "Java"));
        _ScanDirectory(results, Path.Combine(programFilesX86, "Java"));
        _ScanDirectory(results, Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs"));
    }

    private void _ScanUnix(List<string> results)
    {
        _ScanDirectory(results, "/usr/lib/jvm");
        _ScanDirectory(results, "/usr/java");
        _ScanDirectory(results, "/opt/java");
        _ScanDirectory(results, "/Library/Java/JavaVirtualMachines");
        
        if (File.Exists("/usr/bin/java")) results.Add("/usr/bin/java");
        if (File.Exists("/usr/local/bin/java")) results.Add("/usr/local/bin/java");
    }

    private void _ScanDirectory(List<string> results, string dir)
    {
        try
        {
            if (!Directory.Exists(dir)) return;
            
            foreach (var subDir in Directory.GetDirectories(dir))
            {
                var javaExe = Path.Combine(subDir, "bin", 
                    Environment.OSVersion.Platform == PlatformID.Win32NT ? "java.exe" : "java");
                if (File.Exists(javaExe))
                {
                    results.Add(javaExe);
                }
            }
        }
        catch
        {
        }
    }
}