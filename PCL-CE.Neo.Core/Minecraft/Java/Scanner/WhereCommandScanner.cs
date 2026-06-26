using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;

namespace PCL_CE.Neo.Core.Minecraft.Java;

/// <summary>
/// Scanner that uses system commands (where/which) to find Java.
/// </summary>
public sealed class WhereCommandScanner : IJavaScanner
{
    private readonly ILogger? _logger;

    public string Name => "WhereCommand";

    public WhereCommandScanner() : this(null)
    {
    }

    public WhereCommandScanner(ILogger? logger)
    {
        _logger = logger;
    }

    public void Scan(ICollection<string> results)
    {
        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                ScanWithWhereCommand(results);
            }
            else
            {
                ScanWithWhichCommand(results);
            }

            _logger?.LogInformation("WhereCommandScanner found {Count} Java installations", results.Count);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "WhereCommandScanner failed");
        }
    }

    private void ScanWithWhereCommand(ICollection<string> results)
    {
        try
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "where",
                    Arguments = "java",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };

            process.Start();
            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit(5000);

            if (process.ExitCode == 0)
            {
                var paths = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var path in paths)
                {
                    var trimmed = path.Trim();
                    if (!string.IsNullOrEmpty(trimmed) && !results.Contains(trimmed))
                    {
                        results.Add(trimmed);
                        _logger?.LogDebug("Found Java via where command: {Path}", trimmed);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger?.LogDebug("where command failed: {Message}", ex.Message);
        }
    }

    private void ScanWithWhichCommand(ICollection<string> results)
    {
        try
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "which",
                    Arguments = "java",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };

            process.Start();
            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit(5000);

            if (process.ExitCode == 0)
            {
                var path = output.Trim();
                if (!string.IsNullOrEmpty(path) && !results.Contains(path))
                {
                    results.Add(path);
                    _logger?.LogDebug("Found Java via which command: {Path}", path);
                }
            }
        }
        catch (Exception ex)
        {
            _logger?.LogDebug("which command failed: {Message}", ex.Message);
        }

        TryScanCommonLinuxPaths(results);
    }

    private void TryScanCommonLinuxPaths(ICollection<string> results)
    {
        var commonPaths = new[]
        {
            "/usr/bin/java",
            "/usr/lib/jvm/default-java/bin/java",
            "/usr/lib/jvm/java-8-openjdk/bin/java",
            "/usr/lib/jvm/java-11-openjdk/bin/java",
            "/usr/lib/jvm/java-17-openjdk/bin/java",
            "/usr/lib/jvm/java-21-openjdk/bin/java"
        };

        foreach (var path in commonPaths)
        {
            if (System.IO.File.Exists(path) && !results.Contains(path))
            {
                results.Add(path);
                _logger?.LogDebug("Found Java in common Linux path: {Path}", path);
            }
        }
    }
}