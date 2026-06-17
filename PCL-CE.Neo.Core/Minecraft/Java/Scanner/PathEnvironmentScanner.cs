using System.Collections.Generic;
using System.IO;

namespace PCL_CE.Neo.Core.Minecraft.Java.Scanner;

public class PathEnvironmentScanner : IJavaScanner
{
    public void Scan(ICollection<string> results)
    {
        var pathEnv = System.Environment.GetEnvironmentVariable("PATH");
        if (string.IsNullOrEmpty(pathEnv))
            return;

        var paths = pathEnv.Split(Path.PathSeparator);
        foreach (var path in paths)
        {
            try
            {
                var javaExe = Path.Combine(path, "java" + (OperatingSystem.IsWindows() ? ".exe" : ""));
                if (File.Exists(javaExe))
                    results.Add(javaExe);
            }
            catch
            {
            }
        }
    }
}