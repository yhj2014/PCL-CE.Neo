using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;

namespace PCL_CE.Neo.Core.Minecraft.Java;

/// <summary>
/// Scanner that searches Windows registry for Java installations.
/// Only works on Windows platform.
/// </summary>
public sealed class RegistryJavaScanner : IJavaScanner
{
    private readonly ILogger? _logger;

    public string Name => "Registry";

    public RegistryJavaScanner() : this(null)
    {
    }

    public RegistryJavaScanner(ILogger? logger)
    {
        _logger = logger;
    }

    public void Scan(ICollection<string> results)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            _logger?.LogDebug("RegistryJavaScanner skipped (not Windows platform)");
            return;
        }

        try
        {
            ScanJavaSoftRegistry(results);
            ScanBrandRegistry(results);
            ScanUserRegistry(results);

            _logger?.LogInformation("RegistryJavaScanner found {Count} Java installations", results.Count);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "RegistryJavaScanner failed");
        }
    }

    private void ScanJavaSoftRegistry(ICollection<string> results)
    {
        var registryPaths = new[]
        {
            "SOFTWARE\\JavaSoft\\Java Development Kit",
            "SOFTWARE\\JavaSoft\\Java Runtime Environment",
            "SOFTWARE\\WOW6432Node\\JavaSoft\\Java Development Kit",
            "SOFTWARE\\WOW6432Node\\JavaSoft\\Java Runtime Environment",
            "SOFTWARE\\JavaSoft\\JDK",
            "SOFTWARE\\WOW6432Node\\JavaSoft\\JDK"
        };

        foreach (var regPath in registryPaths)
        {
            TryScanRegistryKey(regPath, results, Microsoft.Win32.Registry.LocalMachine);
        }
    }

    private void ScanBrandRegistry(ICollection<string> results)
    {
        var brandPaths = new[]
        {
            "SOFTWARE\\Azul Systems\\Zulu",
            "SOFTWARE\\BellSoft\\Liberica",
            "SOFTWARE\\Eclipse Adoptium\\JDK",
            "SOFTWARE\\Microsoft\\JDK",
            "SOFTWARE\\Amazon\\Corretto"
        };

        foreach (var regPath in brandPaths)
        {
            TryScanRegistryKey(regPath, results, Microsoft.Win32.Registry.LocalMachine);
        }
    }

    private void ScanUserRegistry(ICollection<string> results)
    {
        var userRegistryPaths = new[]
        {
            "SOFTWARE\\JavaSoft\\Java Development Kit",
            "SOFTWARE\\JavaSoft\\Java Runtime Environment",
            "SOFTWARE\\JavaSoft\\JDK"
        };

        foreach (var regPath in userRegistryPaths)
        {
            TryScanRegistryKey(regPath, results, Microsoft.Win32.Registry.CurrentUser);
        }
    }

    private void TryScanRegistryKey(string regPath, ICollection<string> results, Microsoft.Win32.RegistryKey rootKey)
    {
        try
        {
            using var regKey = rootKey.OpenSubKey(regPath);
            if (regKey == null)
                return;

            foreach (var subKeyName in regKey.GetSubKeyNames())
            {
                using var subKey = regKey.OpenSubKey(subKeyName);
                if (subKey == null)
                    continue;

                var javaHome = subKey.GetValue("JavaHome") as string;
                if (string.IsNullOrEmpty(javaHome))
                    continue;

                var invalidChars = Path.GetInvalidPathChars();
                if (javaHome.IndexOfAny(invalidChars) >= 0)
                    continue;

                var javaExePath = Path.Combine(javaHome, "bin", "java.exe");
                if (File.Exists(javaExePath) && !results.Contains(javaExePath))
                {
                    results.Add(javaExePath);
                    _logger?.LogDebug("Found Java in registry: {Path}", javaExePath);
                }
            }
        }
        catch (System.PlatformNotSupportedException)
        {
            _logger?.LogDebug("Registry not supported on this platform: {Path}", regPath);
        }
        catch (UnauthorizedAccessException)
        {
            _logger?.LogDebug("Registry access denied: {Path}", regPath);
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Error scanning registry: {Path}", regPath);
        }
    }
}