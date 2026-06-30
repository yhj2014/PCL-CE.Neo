using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace PCL_CE.Neo.Core.Minecraft.Java;

public class JavaInstallation
{
    private readonly ILogger<JavaInstallation> _logger;

    public JavaInstallation(ILogger<JavaInstallation> logger)
    {
        _logger = logger;
    }

    public async Task<List<JavaEntry>> ScanInstallationsAsync()
    {
        var results = new List<JavaEntry>();

        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                results.AddRange(await ScanWindowsAsync());
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                results.AddRange(await ScanLinuxAsync());
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                results.AddRange(await ScanMacOSAsync());
            }

            results = results.DistinctBy(j => j.Path).ToList();
            _logger.LogInformation("Found {Count} Java installations", results.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to scan Java installations");
        }

        return results;
    }

    private async Task<List<JavaEntry>> ScanWindowsAsync()
    {
        var entries = new List<JavaEntry>();

        try
        {
            var javaHome = Environment.GetEnvironmentVariable("JAVA_HOME");
            if (!string.IsNullOrEmpty(javaHome))
            {
                var javaExe = Path.Combine(javaHome, "bin", "java.exe");
                if (File.Exists(javaExe))
                {
                    var version = await GetJavaVersionAsync(javaExe);
                    entries.Add(new JavaEntry(javaExe, version, "System", GetArchitecture(), javaHome));
                }
            }

            var paths = new[]
            {
                @"C:\Program Files\Java",
                @"C:\Program Files (x86)\Java",
                @"C:\Program Files\Eclipse Adoptium",
                @"C:\Program Files (x86)\Eclipse Adoptium",
                @"C:\Program Files\Microsoft\jdk"
            };

            foreach (var path in paths)
            {
                if (Directory.Exists(path))
                {
                    foreach (var dir in Directory.GetDirectories(path))
                    {
                        var javaExe = Path.Combine(dir, "bin", "java.exe");
                        if (File.Exists(javaExe))
                        {
                            var version = await GetJavaVersionAsync(javaExe);
                            entries.Add(new JavaEntry(javaExe, version, GetVendorFromPath(dir), GetArchitecture(), dir));
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to scan Java on Windows");
        }

        return entries;
    }

    private async Task<List<JavaEntry>> ScanLinuxAsync()
    {
        var entries = new List<JavaEntry>();

        try
        {
            var javaHome = Environment.GetEnvironmentVariable("JAVA_HOME");
            if (!string.IsNullOrEmpty(javaHome))
            {
                var javaExe = Path.Combine(javaHome, "bin", "java");
                if (File.Exists(javaExe))
                {
                    var version = await GetJavaVersionAsync(javaExe);
                    entries.Add(new JavaEntry(javaExe, version, "System", GetArchitecture(), javaHome));
                }
            }

            var paths = new[]
            {
                "/usr/lib/jvm",
                "/usr/local/java",
                "/opt/java",
                "/opt/jdk"
            };

            foreach (var path in paths)
            {
                if (Directory.Exists(path))
                {
                    foreach (var dir in Directory.GetDirectories(path))
                    {
                        var javaExe = Path.Combine(dir, "bin", "java");
                        if (File.Exists(javaExe))
                        {
                            var version = await GetJavaVersionAsync(javaExe);
                            entries.Add(new JavaEntry(javaExe, version, GetVendorFromPath(dir), GetArchitecture(), dir));
                        }
                    }
                }
            }

            var whichJava = await RunCommandAsync("which", "java");
            if (!string.IsNullOrEmpty(whichJava))
            {
                var resolvedPath = await RunCommandAsync("readlink", "-f", whichJava.Trim());
                if (!string.IsNullOrEmpty(resolvedPath))
                {
                    resolvedPath = resolvedPath.Trim();
                    if (!entries.Any(e => e.Path == resolvedPath) && File.Exists(resolvedPath))
                    {
                        var version = await GetJavaVersionAsync(resolvedPath);
                        var homeDir = Path.GetDirectoryName(Path.GetDirectoryName(resolvedPath));
                        entries.Add(new JavaEntry(resolvedPath, version, "System", GetArchitecture(), homeDir ?? string.Empty));
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to scan Java on Linux");
        }

        return entries;
    }

    private async Task<List<JavaEntry>> ScanMacOSAsync()
    {
        var entries = new List<JavaEntry>();

        try
        {
            var javaHome = Environment.GetEnvironmentVariable("JAVA_HOME");
            if (!string.IsNullOrEmpty(javaHome))
            {
                var javaExe = Path.Combine(javaHome, "bin", "java");
                if (File.Exists(javaExe))
                {
                    var version = await GetJavaVersionAsync(javaExe);
                    entries.Add(new JavaEntry(javaExe, version, "System", GetArchitecture(), javaHome));
                }
            }

            var paths = new[]
            {
                "/Library/Java/JavaVirtualMachines",
                "/usr/lib/jvm",
                "/opt/homebrew/opt"
            };

            foreach (var path in paths)
            {
                if (Directory.Exists(path))
                {
                    foreach (var dir in Directory.GetDirectories(path))
                    {
                        var javaExe = Path.Combine(dir, "Contents", "Home", "bin", "java");
                        if (!File.Exists(javaExe))
                            javaExe = Path.Combine(dir, "bin", "java");

                        if (File.Exists(javaExe))
                        {
                            var version = await GetJavaVersionAsync(javaExe);
                            var home = Path.GetDirectoryName(Path.GetDirectoryName(javaExe));
                            entries.Add(new JavaEntry(javaExe, version, GetVendorFromPath(dir), GetArchitecture(), home ?? dir));
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to scan Java on macOS");
        }

        return entries;
    }

    public async Task<string> GetJavaVersionAsync(string javaPath)
    {
        try
        {
            var result = await RunCommandAsync(javaPath, "-version");
            if (!string.IsNullOrEmpty(result))
            {
                var lines = result.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines)
                {
                    if (line.StartsWith("java version") || line.StartsWith("openjdk version"))
                    {
                        var parts = line.Split('"');
                        if (parts.Length >= 2)
                            return parts[1];
                    }
                }
            }
            return "Unknown";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get Java version for {Path}", javaPath);
            return "Unknown";
        }
    }

    private async Task<string> RunCommandAsync(string command, params string[] args)
    {
        try
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = command,
                Arguments = string.Join(" ", args),
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = processStartInfo };
            process.Start();
            await process.WaitForExitAsync();

            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();

            return string.IsNullOrEmpty(output) ? error : output;
        }
        catch
        {
            return string.Empty;
        }
    }

    private string GetArchitecture()
    {
        return Environment.Is64BitProcess ? "64-bit" : "32-bit";
    }

    private string GetVendorFromPath(string path)
    {
        var name = Path.GetFileName(path).ToLowerInvariant();
        if (name.Contains("adoptopenjdk") || name.Contains("adoptium"))
            return "Eclipse Adoptium";
        if (name.Contains("openjdk"))
            return "OpenJDK";
        if (name.Contains("oracle"))
            return "Oracle";
        if (name.Contains("microsoft"))
            return "Microsoft";
        if (name.Contains("ibm"))
            return "IBM";
        if (name.Contains("zulu"))
            return "Azul Zulu";
        return "Unknown";
    }

    public bool IsJavaVersionCompatible(string javaVersion, string minecraftVersion)
    {
        if (string.IsNullOrWhiteSpace(javaVersion) || string.IsNullOrWhiteSpace(minecraftVersion))
            return false;

        try
        {
            var javaMajor = ParseJavaMajorVersion(javaVersion);
            var mcMajor = ParseMinecraftMajorVersion(minecraftVersion);

            if (mcMajor >= 1 && mcMajor <= 7)
                return javaMajor >= 8;
            if (mcMajor >= 8 && mcMajor <= 16)
                return javaMajor >= 8 && javaMajor <= 17;
            if (mcMajor == 17)
                return javaMajor >= 17;
            if (mcMajor >= 18 && mcMajor <= 20)
                return javaMajor >= 18;
            if (mcMajor >= 21)
                return javaMajor >= 21;

            return javaMajor >= 8;
        }
        catch
        {
            return false;
        }
    }

    private int ParseJavaMajorVersion(string version)
    {
        if (version.StartsWith("1."))
        {
            var parts = version.Substring(2).Split('.');
            if (int.TryParse(parts[0], out var major))
                return major;
        }
        else
        {
            var parts = version.Split('.', '-');
            if (int.TryParse(parts[0], out var major))
                return major;
        }
        return 8;
    }

    private int ParseMinecraftMajorVersion(string version)
    {
        var parts = version.Split('.');
        if (int.TryParse(parts[0], out var major))
            return major;
        return 1;
    }
}