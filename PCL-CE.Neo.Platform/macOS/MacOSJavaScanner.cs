using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Logging;
using PCL_CE.Neo.Core.Abstractions;

namespace PCL_CE.Neo.Platform.macOS;

public class MacOSJavaScanner : IJavaScanner
{
    private readonly ILogger<MacOSJavaScanner> _logger;
    private static readonly string[] DefaultJavaPaths = new[]
    {
        "/Library/Java/JavaVirtualMachines",
        "/System/Library/Java/JavaVirtualMachines",
        "/opt/homebrew/opt",
        "/usr/local/opt",
        "/usr/lib/jvm"
    };

    public MacOSJavaScanner() : this(Microsoft.Extensions.Logging.Abstractions.NullLogger<MacOSJavaScanner>.Instance) { }

    public MacOSJavaScanner(ILogger<MacOSJavaScanner> logger)
    {
        _logger = logger;
        _logger.LogDebug("MacOSJavaScanner initialized");
    }

    public IEnumerable<string> ScanJavaPaths()
    {
        var javaPaths = new HashSet<string>(StringComparer.Ordinal);

        try
        {
            _logger.LogInformation("Starting Java path scan on macOS");

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
                    _logger.LogWarning(ex, "Error scanning path: {Path}", basePath);
                }
            }

            try
            {
                using var javaHomeProcess = Process.Start(new ProcessStartInfo
                {
                    FileName = "/usr/libexec/java_home",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                });

                if (javaHomeProcess != null)
                {
                    var output = javaHomeProcess.StandardOutput.ReadToEnd().Trim();
                    javaHomeProcess.WaitForExit(3000);

                    if (!string.IsNullOrWhiteSpace(output) && Directory.Exists(output))
                    {
                        if (IsValidJavaPath(output))
                        {
                            javaPaths.Add(output);
                            _logger.LogDebug("Found java_home: {Path}", output);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "java_home command not available or failed");
            }

            try
            {
                using var allJavaProcess = Process.Start(new ProcessStartInfo
                {
                    FileName = "/usr/libexec/java_home",
                    Arguments = "-V",
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                });

                if (allJavaProcess != null)
                {
                    var output = allJavaProcess.StandardError.ReadToEnd();
                    allJavaProcess.WaitForExit(5000);

                    var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                    foreach (var line in lines)
                    {
                        try
                        {
                            var trimmed = line.Trim();
                            var parenStart = trimmed.IndexOf('(');
                            var parenEnd = trimmed.IndexOf(')');
                            if (parenStart >= 0 && parenEnd > parenStart)
                            {
                                var pathPart = trimmed.Substring(parenEnd + 1).Trim();
                                if (pathPart.StartsWith('"') && pathPart.EndsWith('"'))
                                {
                                    pathPart = pathPart.Substring(1, pathPart.Length - 2);
                                }
                                if (!string.IsNullOrWhiteSpace(pathPart) && Directory.Exists(pathPart))
                                {
                                    var parentDir = Directory.GetParent(pathPart)?.FullName ?? pathPart;
                                    var targetDir = Directory.Exists(Path.Combine(parentDir, "bin"))
                                        ? parentDir
                                        : pathPart;
                                    if (IsValidJavaPath(targetDir))
                                    {
                                        javaPaths.Add(targetDir);
                                        _logger.LogDebug("Found Java via java_home -V: {Path}", targetDir);
                                    }
                                }
                            }
                        }
                        catch (Exception innerEx)
                        {
                            _logger.LogDebug(innerEx, "Error parsing java_home -V line: {Line}", line);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "java_home -V command failed");
            }

            var javaHomeEnv = Environment.GetEnvironmentVariable("JAVA_HOME");
            if (!string.IsNullOrWhiteSpace(javaHomeEnv) && Directory.Exists(javaHomeEnv))
            {
                if (IsValidJavaPath(javaHomeEnv))
                {
                    javaPaths.Add(javaHomeEnv);
                    _logger.LogDebug("Found JAVA_HOME: {Path}", javaHomeEnv);
                }
            }

            var envPath = Environment.GetEnvironmentVariable("PATH");
            if (!string.IsNullOrWhiteSpace(envPath))
            {
                foreach (var dir in envPath.Split(':', StringSplitOptions.RemoveEmptyEntries))
                {
                    try
                    {
                        if (Directory.Exists(dir) &&
                            File.Exists(Path.Combine(dir, "java")))
                        {
                            var javaDir = Directory.GetParent(dir)?.FullName ?? dir;
                            if (IsValidJavaPath(javaDir))
                            {
                                javaPaths.Add(javaDir);
                            }
                            else if (IsValidJavaPath(dir))
                            {
                                javaPaths.Add(dir);
                            }
                        }
                    }
                    catch { }
                }
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
                    var javaBin = Path.Combine(dir, "bin", "java");
                    var javaHomeBin = Path.Combine(dir, "Contents", "Home", "bin", "java");
                    if (File.Exists(javaBin))
                    {
                        paths.Add(dir);
                        _logger.LogDebug("Found Java installation: {Dir}", dir);
                    }
                    else if (File.Exists(javaHomeBin))
                    {
                        paths.Add(Path.Combine(dir, "Contents", "Home"));
                        _logger.LogDebug("Found Java installation (macOS bundle): {Dir}", dir);
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
            var javaBin = Path.Combine(path, "bin", "java");
            var altJavaBin = Path.Combine(path, "java");
            var exists = File.Exists(javaBin) || File.Exists(altJavaBin);

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
