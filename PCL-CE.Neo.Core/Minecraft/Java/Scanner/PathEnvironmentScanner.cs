using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Logging;

namespace PCL_CE.Neo.Core.Minecraft.Java.Scanner;

public class PathEnvironmentScanner(ILogger<PathEnvironmentScanner> logger) : IJavaScanner
{
    public void Scan(ICollection<string> results)
    {
        try
        {
            var pathVar = Environment.GetEnvironmentVariable("PATH");
            if (string.IsNullOrEmpty(pathVar)) return;

            var separator = OperatingSystem.IsWindows() ? ';' : ':';
            foreach (var dir in pathVar.Split(separator, StringSplitOptions.RemoveEmptyEntries))
            {
                if (!Directory.Exists(dir)) continue;

                var javaExe = Path.Combine(dir, "java.exe");
                if (File.Exists(javaExe)) results.Add(javaExe);

                var javaBin = Path.Combine(dir, "bin", "java.exe");
                if (File.Exists(javaBin)) results.Add(javaBin);

                if (!OperatingSystem.IsWindows())
                {
                    var javaUnix = Path.Combine(dir, "java");
                    if (File.Exists(javaUnix)) results.Add(javaUnix);

                    javaUnix = Path.Combine(dir, "bin", "java");
                    if (File.Exists(javaUnix)) results.Add(javaUnix);
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "PATH环境变量扫描失败");
        }
    }
}