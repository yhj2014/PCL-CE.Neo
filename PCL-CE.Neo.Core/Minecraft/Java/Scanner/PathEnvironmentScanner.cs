using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;

namespace PCL_CE.Neo.Core.Minecraft.Java;

/// <summary>
/// Scanner that searches for Java in PATH environment variable.
/// </summary>
public sealed class PathEnvironmentScanner : IJavaScanner
{
    private readonly ILogger? _logger;

    public string Name => "PathEnvironment";

    public PathEnvironmentScanner() : this(null)
    {
    }

    public PathEnvironmentScanner(ILogger? logger)
    {
        _logger = logger;
    }

    public void Scan(ICollection<string> results)
    {
        try
        {
            var pathEnv = Environment.GetEnvironmentVariable("PATH");
            if (string.IsNullOrEmpty(pathEnv))
            {
                _logger?.LogDebug("PATH environment variable is empty");
                return;
            }

            var separator = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ';' : ':';
            var paths = pathEnv.Split(separator, StringSplitOptions.RemoveEmptyEntries);

            foreach (var path in paths)
            {
                var trimmedPath = path.Trim();
                if (string.IsNullOrEmpty(trimmedPath))
                    continue;

                TryFindJavaInPath(trimmedPath, results);
            }

            _logger?.LogInformation("PathEnvironmentScanner found {Count} Java installations", results.Count);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "PathEnvironmentScanner failed");
        }
    }

    private void TryFindJavaInPath(string pathDir, ICollection<string> results)
    {
        try
        {
            if (!Directory.Exists(pathDir))
                return;

            string javaExe;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                javaExe = Path.Combine(pathDir, "java.exe");
            }
            else
            {
                javaExe = Path.Combine(pathDir, "java");
            }

            if (File.Exists(javaExe))
            {
                if (!results.Contains(javaExe))
                {
                    results.Add(javaExe);
                    _logger?.LogDebug("Found Java in PATH: {Path}", javaExe);
                }
                return;
            }

            var parentDir = Path.GetDirectoryName(pathDir);
            if (parentDir != null && Directory.Exists(parentDir))
            {
                javaExe = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                    ? Path.Combine(parentDir, "bin", "java.exe")
                    : Path.Combine(parentDir, "bin", "java");

                if (File.Exists(javaExe) && !results.Contains(javaExe))
                {
                    results.Add(javaExe);
                    _logger?.LogDebug("Found Java in PATH parent: {Path}", javaExe);
                }
            }
        }
        catch (Exception ex)
        {
            _logger?.LogDebug("Error checking PATH entry {Path}: {Message}", pathDir, ex.Message);
        }
    }
}