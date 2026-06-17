using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Logging;

namespace PCL_CE.Neo.Core.Minecraft.Java.Scanner;

public class MicrosoftStoreJavaScanner(ILogger<MicrosoftStoreJavaScanner>? logger = null) : IJavaScanner
{
    private const string StorePackagePath =
        @"Packages\Microsoft.4297127D64EC6_8wekyb3d8bbwe\LocalCache\Local\runtime";

    public void Scan(ICollection<string> results)
    {
        if (!OperatingSystem.IsWindows())
        {
            logger?.LogDebug("[Java] Microsoft Store Java 扫描器仅支持 Windows");
            return;
        }

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
                            logger?.LogInformation("[Java] 检测到 Microsoft Store Java: {JavaExe}", javaExe);
                            results.Add(javaExe);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "[Java] Microsoft Store Java 扫描失败");
        }
    }
}