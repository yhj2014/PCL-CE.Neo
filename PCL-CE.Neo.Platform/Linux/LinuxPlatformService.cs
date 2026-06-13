using System.Runtime.InteropServices;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using PCL_CE.Neo.Core.Abstractions;

namespace PCL_CE.Neo.Platform.Linux;

public class LinuxPlatformService : IPlatformService
{
    private readonly ILogger<LinuxPlatformService> _logger;

    public LinuxPlatformService(ILogger<LinuxPlatformService> logger)
    {
        _logger = logger;
        _logger.LogDebug("LinuxPlatformService initialized");
    }

    public string PlatformName => "Linux";

    public string OSVersion
    {
        get
        {
            try
            {
                if (File.Exists("/etc/os-release"))
                {
                    var lines = File.ReadAllLines("/etc/os-release");
                    foreach (var line in lines)
                    {
                        if (line.StartsWith("PRETTY_NAME=", StringComparison.Ordinal))
                        {
                            var value = line.Substring("PRETTY_NAME=".Length).Trim('"');
                            return value;
                        }
                    }
                }
                return RuntimeInformation.OSDescription;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to retrieve OS version");
                return "Linux";
            }
        }
    }

    public string Architecture
    {
        get
        {
            try
            {
                return RuntimeInformation.OSArchitecture.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to retrieve architecture");
                return "Unknown";
            }
        }
    }

    public void OpenUrl(string url)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                _logger.LogWarning("OpenUrl called with empty or null url");
                return;
            }

            _logger.LogDebug("Opening URL on Linux: {Url}", url);

            using var process = Process.Start(new ProcessStartInfo
            {
                FileName = "xdg-open",
                Arguments = "\"" + url + "\"",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            });

            if (process != null)
            {
                process.WaitForExit(5000);
                _logger.LogInformation("URL open command executed, exit code: {ExitCode}", process.ExitCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to open URL on Linux: {Url}", url);
        }
    }

    public void OpenFolder(string path)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                _logger.LogWarning("OpenFolder called with empty or null path");
                return;
            }

            if (!Directory.Exists(path))
            {
                _logger.LogWarning("Folder does not exist: {Path}", path);
                return;
            }

            _logger.LogDebug("Opening folder on Linux: {Path}", path);

            using var process = Process.Start(new ProcessStartInfo
            {
                FileName = "xdg-open",
                Arguments = "\"" + path + "\"",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            });

            if (process != null)
            {
                process.WaitForExit(5000);
                _logger.LogInformation("Folder open command executed, exit code: {ExitCode}", process.ExitCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to open folder on Linux: {Path}", path);
        }
    }

    public string GetLocalApplicationDataPath()
    {
        try
        {
            var xdgDataHome = Environment.GetEnvironmentVariable("XDG_DATA_HOME");
            if (!string.IsNullOrWhiteSpace(xdgDataHome) && Directory.Exists(xdgDataHome))
            {
                return xdgDataHome;
            }

            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var fallback = Path.Combine(home, ".local", "share");
            Directory.CreateDirectory(fallback);
            _logger.LogDebug("LocalApplicationData path: {Path}", fallback);
            return fallback;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get local app data path");
            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var fallback = Path.Combine(home, ".local", "share");
            Directory.CreateDirectory(fallback);
            return fallback;
        }
    }

    public string GetTempPath()
    {
        try
        {
            var path = Path.GetTempPath();
            _logger.LogDebug("Temp path: {Path}", path);
            return path;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get temp path");
            var fallback = Path.Combine("/tmp", "pcl-ce-neo");
            Directory.CreateDirectory(fallback);
            return fallback;
        }
    }

    public string GetGameDataPath()
    {
        try
        {
            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var path = Path.Combine(home, ".config", "PCL-CE.Neo", "GameData");
            Directory.CreateDirectory(path);
            _logger.LogDebug("GameData path: {Path}", path);
            return path;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to create game data path");
            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var fallback = Path.Combine(home, "PCL-CE.Neo", "GameData");
            Directory.CreateDirectory(fallback);
            return fallback;
        }
    }
}
