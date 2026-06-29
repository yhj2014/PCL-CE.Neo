using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace PCL_CE.Neo.Core.Minecraft.Java.Scanner;

public class WhereCommandScanner(ILogger<WhereCommandScanner> logger) : IJavaScanner
{
    public void Scan(ICollection<string> results)
    {
        if (!OperatingSystem.IsWindows()) return;

        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "where",
                Arguments = "java",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using var proc = Process.Start(psi);
            if (proc == null) return;

            var output = proc.StandardOutput.ReadToEnd();
            proc.WaitForExit();

            if (proc.ExitCode != 0) return;

            var paths = output.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Trim())
                .Where(p => File.Exists(p));

            foreach (var path in paths)
                results.Add(path);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "where 命令扫描失败");
        }
    }
}