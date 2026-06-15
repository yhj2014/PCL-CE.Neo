using System.Runtime.InteropServices;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using PCL_CE.Neo.Core.Abstractions;

namespace PCL_CE.Neo.Platform.Windows;

public class WindowsPlatformService : IPlatformService
{
    private readonly ILogger<WindowsPlatformService> _logger;

    public WindowsPlatformService() : this(Microsoft.Extensions.Logging.Abstractions.NullLogger<WindowsPlatformService>.Instance)
    {
    }

    public WindowsPlatformService(ILogger<WindowsPlatformService> logger)
    {
        _logger = logger;
        _logger.LogDebug("WindowsPlatformService initialized");
    }

    public string PlatformName => "Windows";

    public string OSVersion
    {
        get
        {
            try
            {
                return Environment.OSVersion.VersionString;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to retrieve OS version");
                return "Unknown";
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

            _logger.LogDebug("Opening URL: {Url}", url);

            using var process = Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true,
                CreateNoWindow = true
            });

            if (process != null)
            {
                _logger.LogInformation("URL opened successfully, process ID: {ProcessId}", process.Id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to open URL: {Url}", url);
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

            _logger.LogDebug("Opening folder: {Path}", path);

            using var process = Process.Start(new ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments = "\"" + path + "\"",
                UseShellExecute = false,
                CreateNoWindow = true
            });

            if (process != null)
            {
                _logger.LogInformation("Folder opened successfully, process ID: {ProcessId}", process.Id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to open folder: {Path}", path);
        }
    }

    public string GetLocalApplicationDataPath()
    {
        try
        {
            var path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            _logger.LogDebug("LocalApplicationData path: {Path}", path);
            return path;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get LocalApplicationData path");
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "AppData", "Local");
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
            var fallback = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "AppData", "Local", "Temp");
            Directory.CreateDirectory(fallback);
            return fallback;
        }
    }

    public string GetGameDataPath()
    {
        try
        {
            var basePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var path = Path.Combine(basePath, "PCL-CE.Neo", "GameData");
            Directory.CreateDirectory(path);
            _logger.LogDebug("GameData path: {Path}", path);
            return path;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to create game data path");
            var fallback = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "PCL-CE.Neo", "GameData");
            Directory.CreateDirectory(fallback);
            return fallback;
        }
    }
}
