using System.Runtime.InteropServices;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using PCL_CE.Neo.Core.Abstractions;

namespace PCL_CE.Neo.Platform.macOS;

public class MacOSPlatformService : IPlatformService
{
    private readonly ILogger<MacOSPlatformService> _logger;

    public MacOSPlatformService() : this(Microsoft.Extensions.Logging.Abstractions.NullLogger<MacOSPlatformService>.Instance)
    {
    }

    public MacOSPlatformService(ILogger<MacOSPlatformService> logger)
    {
        _logger = logger;
        _logger.LogDebug("MacOSPlatformService initialized");
    }

    public string PlatformName => "macOS";

    public string OSVersion
    {
        get
        {
            try
            {
                return RuntimeInformation.OSDescription;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to retrieve OS version");
                return "macOS";
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

            _logger.LogDebug("Opening URL on macOS: {Url}", url);

            using var process = Process.Start(new ProcessStartInfo
            {
                FileName = "open",
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
            _logger.LogError(ex, "Failed to open URL on macOS: {Url}", url);
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

            _logger.LogDebug("Opening folder on macOS: {Path}", path);

            using var process = Process.Start(new ProcessStartInfo
            {
                FileName = "open",
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
            _logger.LogError(ex, "Failed to open folder on macOS: {Path}", path);
        }
    }

    public string GetLocalApplicationDataPath()
    {
        try
        {
            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var path = Path.Combine(home, "Library", "Application Support");
            _logger.LogDebug("LocalApplicationData path: {Path}", path);
            return path;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get local app data path");
            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var fallback = Path.Combine(home, "Library", "Application Support");
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
            var path = Path.Combine(home, "Library", "Application Support", "PCL-CE.Neo", "GameData");
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
