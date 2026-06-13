using System.Diagnostics;
using Microsoft.Extensions.Logging;
using PCL_CE.Neo.Core.Abstractions;

namespace PCL_CE.Neo.Platform.Linux;

public class LinuxJavaScanner : IJavaScanner
{
    private readonly ILogger<LinuxJavaScanner> _logger;
    private static readonly string[] DefaultJavaPaths = new[]
    {
        "/usr/lib/jvm",
        "/usr/java",
        "/opt/java",
        "/opt/jdk",
        "/opt/sun-java",
        "/usr/local/java"
    };

    public LinuxJavaScanner(ILogger<LinuxJavaScanner> logger)
    {
        _logger = logger;
        _logger.LogDebug("LinuxJavaScanner initialized");
    }

    public IEnumerable<string> ScanJavaPaths()
    {
        var javaPaths = new HashSet<string>(StringComparer.Ordinal);

        try
        {
            _logger.LogInformation("Starting Java path scan on Linux");

            foreach (var basePath in DefaultJavaPaths)
            {
                try
                {
                    if (Directory.Exists(basePath))
                    {
                        foreach (var found in ScanDirectory(basePath))
                        {
                            javaPaths.Add(found);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error scanning default path: {Path}", basePath);
                }
            }

            var javaHome = Environment.GetEnvironmentVariable("JAVA_HOME");
            if (!string.IsNullOrWhiteSpace(javaHome) && Directory.Exists(javaHome))
            {
                if (IsValidJavaPath(javaHome))
                {
                    javaPaths.Add(javaHome);
                    _logger.LogDebug("Found JAVA_HOME: {Path}", javaHome);
                }
            }

            var userHome = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            if (!string.IsNullOrWhiteSpace(userHome))
            {
                var userJdks = Path.Combine(userHome, ".jdks");
                if (Directory.Exists(userJdks))
                {
                    foreach (var found in ScanDirectory(userJdks))
                    {
                        javaPaths.Add(found);
                    }
                }

                var sdkMan = Path.Combine(userHome, ".sdkman", "candidates", "java");
                if (Directory.Exists(sdkMan))
                {
                    foreach (var found in ScanDirectory(sdkMan))
                    {
                        javaPaths.Add(found);
                    }
                }
            }

            try
            {
                using var process = Process.Start(new ProcessStartInfo
                {
                    FileName = "which",
                    Arguments = "java",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                });

                if (process != null)
                {
                    var output = process.StandardOutput.ReadToEnd().Trim();
                    process.WaitForExit(5000);

                    if (!string.IsNullOrWhiteSpace(output) && File.Exists(output))
                    {
                        var javaDir = Directory.GetParent(Path.GetDirectoryName(output) ?? output)?.FullName
                                      ?? Path.GetDirectoryName(output) ?? output;

                        if (IsValidJavaPath(javaDir))
                        {
                            javaPaths.Add(javaDir);
                            _logger.LogDebug("Found PATH Java: {Path}", javaDir);
                        }
                        else if (IsValidJavaPath(Path.GetDirectoryName(output) ?? output))
                        {
                            javaPaths.Add(Path.GetDirectoryName(output) ?? output);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "which java search failed");
            }

            _logger.LogInformation("Java scan complete. {Count} installations found", javaPaths.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fatal error during Java path scan");
        }

        return javaPaths.ToList();
    }

    public IEnumerable<string> ScanDirectory(string directory)
    {
        var paths = new List<string>();

        try
        {
            if (!Directory.Exists(directory))
                return paths;

            _logger.LogDebug("Scanning directory: {Directory}", directory);

            foreach (var dir in Directory.GetDirectories(directory))
            {
                try
                {
                    var javaExe = Path.Combine(dir, "bin", "java");
                    if (File.Exists(javaExe))
                    {
                        paths.Add(dir);
                        _logger.LogDebug("Found Java installation: {Dir}", dir);
                    }
                    else
                    {
                        foreach (var nested in ScanDirectory(dir))
                        {
                            paths.Add(nested);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogDebug(ex, "Error scanning subdirectory: {Dir}", dir);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to scan directory: {Directory}", directory);
        }

        return paths;
    }

    public bool IsValidJavaPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return false;
        }

        try
        {
            var javaExe = Path.Combine(path, "bin", "java");
            var exists = File.Exists(javaExe);

            if (exists)
            {
                _logger.LogDebug("Validated Java path: {Path}", path);
            }

            return exists;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error validating Java path: {Path}", path);
            return false;
        }
    }
}
