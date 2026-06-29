using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Logging;

namespace PCL_CE.Neo.Core.Minecraft.Java.Scanner;

public class MicrosoftStoreJavaScanner(ILogger<MicrosoftStoreJavaScanner> logger) : IJavaScanner
{
    private const string StorePackagePath =
        @"Packages\Microsoft.4297127D64EC6_8wekyb3d8bbwe\LocalCache\Local\runtime";

    public void Scan(ICollection<string> results)
    {
        try
        {
            var basePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                StorePackagePath);

            if (!Directory.Exists(basePath)) return;

            foreach (var runtimeDir in Directory.EnumerateDirectories(basePath))
            {
                if (!Path.GetFileName(runtimeDir).StartsWith("java-runtime", StringComparison.OrdinalIgnoreCase))
                    continue;

                foreach (var archDir in Directory.EnumerateDirectories(runtimeDir))
                {
                    foreach (var versionDir in Directory.EnumerateDirectories(archDir))
                    {
                        var javaExe = Path.Combine(versionDir, "bin", "java.exe");
                        if (File.Exists(javaExe))
                        {
                            logger.LogInformation("检测到 Microsoft Store Java: {Path}", javaExe);
                            results.Add(javaExe);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Microsoft Store Java 扫描失败");
        }
    }
}