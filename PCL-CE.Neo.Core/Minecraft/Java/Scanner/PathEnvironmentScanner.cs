using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PCL_CE.Neo.Core.Minecraft.Java.Scanner;

public class PathEnvironmentScanner : IJavaScanner
{
    public void Scan(List<string> results)
    {
        try
        {
            var pathEnv = Environment.GetEnvironmentVariable("PATH");
            if (string.IsNullOrEmpty(pathEnv)) return;

            var paths = pathEnv.Split(Path.PathSeparator);
            foreach (var path in paths)
            {
                try
                {
                    var javaExe = Path.Combine(path, 
                        Environment.OSVersion.Platform == PlatformID.Win32NT ? "java.exe" : "java");
                    if (File.Exists(javaExe))
                    {
                        results.Add(javaExe);
                    }
                }
                catch
                {
                }
            }
        }
        catch
        {
        }
    }
}