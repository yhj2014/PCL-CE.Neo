using System.Runtime.InteropServices;
using Microsoft.Win32;
using Microsoft.Extensions.Logging;
using PCL_CE.Neo.Core.Abstractions;

namespace PCL_CE.Neo.Platform.Windows;

public class WindowsJavaScanner : IJavaScanner
{
    private readonly ILogger<WindowsJavaScanner> _logger;
    private static readonly string[] DefaultJavaPaths = new[]
    {
        @"C:\Program Files\Java",
        @"C:\Program Files (x86)\Java",
        @"C:\Program Files\Eclipse Adoptium",
        @"C:\Program Files\Amazon Corretto",
        @"C:\Program Files\Microsoft",
        @"C:\Program Files\BellSoft"
    };

    public WindowsJavaScanner() : this(Microsoft.Extensions.Logging.Abstractions.NullLogger<WindowsJavaScanner>.Instance) { }

    public WindowsJavaScanner(ILogger<WindowsJavaScanner> logger)
    {
        _logger = logger;
        _logger.LogDebug("WindowsJavaScanner initialized");
    }

    public IEnumerable<string> ScanJavaPaths()
    {
        var javaPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        try
        {
            _logger.LogInformation("Starting Java path scan on Windows");

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

            var javaHome = Environment.GetEnvironmentVariable("JAVA_HOME");
            if (!string.IsNullOrWhiteSpace(javaHome) && Directory.Exists(javaHome))
            {
                if (IsValidJavaPath(javaHome))
                {
                    javaPaths.Add(javaHome);
                    _logger.LogDebug("Found JAVA_HOME: {Path}", javaHome);
                }
            }

            var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            if (!string.IsNullOrWhiteSpace(userProfile))
            {
                var userJdks = Path.Combine(userProfile, ".jdks");
                if (Directory.Exists(userJdks))
                {
                    foreach (var found in ScanDirectory(userJdks))
                    {
                        javaPaths.Add(found);
                    }
                }
            }

            ScanRegistryForJava(javaPaths);

            var envPath = Environment.GetEnvironmentVariable("PATH");
            if (!string.IsNullOrWhiteSpace(envPath))
            {
                foreach (var dir in envPath.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
                {
                    try
                    {
                        if (Directory.Exists(dir) &&
                            File.Exists(Path.Combine(dir, "java.exe")))
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

    private void ScanRegistryForJava(HashSet<string> javaPaths)
    {
        try
        {
            _logger.LogDebug("Scanning registry for Java installations");

            var registryRoots = new[]
            {
                Registry.LocalMachine.OpenSubKey(@"SOFTWARE\JavaSoft\Java Runtime Environment"),
                Registry.LocalMachine.OpenSubKey(@"SOFTWARE\JavaSoft\Java Development Kit"),
                Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Eclipse Adoptium\JRE"),
                Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Eclipse Adoptium\JDK"),
                Registry.CurrentUser.OpenSubKey(@"SOFTWARE\JavaSoft\Java Runtime Environment"),
                Registry.CurrentUser.OpenSubKey(@"SOFTWARE\JavaSoft\Java Development Kit")
            };

            foreach (var key in registryRoots)
            {
                if (key == null) continue;

                using (key)
                {
                    try
                    {
                        foreach (var subKeyName in key.GetSubKeyNames())
                        {
                            try
                            {
                                using var subKey = key.OpenSubKey(subKeyName);
                                if (subKey == null) continue;

                                var javaHome = subKey.GetValue("JavaHome") as string
                                              ?? subKey.GetValue("InstallPath") as string
                                              ?? subKey.GetValue("Path") as string;

                                if (!string.IsNullOrWhiteSpace(javaHome) &&
                                    Directory.Exists(javaHome) &&
                                    IsValidJavaPath(javaHome))
                                {
                                    javaPaths.Add(javaHome);
                                    _logger.LogDebug("Registry Java found: {Path}", javaHome);
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogDebug(ex, "Error reading registry subkey: {Name}", subKeyName);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogDebug(ex, "Error reading registry key");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Registry scan failed");
        }
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
                    var javaExe = Path.Combine(dir, "bin", "java.exe");
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
            var javaExe = Path.Combine(path, "bin", "java.exe");
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
