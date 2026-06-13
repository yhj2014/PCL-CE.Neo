using System.Diagnostics;
using Microsoft.Extensions.Logging;
using PCL_CE.Neo.Core.Abstractions;

namespace PCL_CE.Neo.Platform.Linux;

public class LinuxNotificationService : INotificationService
{
    private readonly ILogger<LinuxNotificationService> _logger;

    public LinuxNotificationService() : this(Microsoft.Extensions.Logging.Abstractions.NullLogger<LinuxNotificationService>.Instance)
    {
    }

    public LinuxNotificationService(ILogger<LinuxNotificationService> logger)
    {
        _logger = logger;
        try
        {
            _logger.LogDebug("LinuxNotificationService initializing");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error during LinuxNotificationService initialization");
        }
    }

    public void ShowNotification(NotificationInfo notification)
    {
        try
        {
            if (notification == null)
            {
                _logger.LogWarning("ShowNotification called with null info");
                return;
            }

            var urgency = notification.Type switch
            {
                NotificationType.Error => "critical",
                NotificationType.Warning => "normal",
                _ => "low"
            };

            using var process = Process.Start(new ProcessStartInfo
            {
                FileName = "notify-send",
                Arguments = $"--urgency={urgency} \"{notification.Title}\" \"{notification.Message}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            });

            if (process != null)
            {
                process.WaitForExit(5000);
            }

            _logger.LogDebug("Notification shown: {Title} - {Message}", notification.Title, notification.Message);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to show notification");
        }
    }

    public void ShowUpdateNotification(string version, string releaseNotes)
    {
        try
        {
            using var process = Process.Start(new ProcessStartInfo
            {
                FileName = "notify-send",
                Arguments = $"--urgency=normal --icon=software-update-available \"Update Available: {version}\" \"{releaseNotes}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            });

            if (process != null)
            {
                process.WaitForExit(5000);
            }

            _logger.LogInformation("Update notification shown for version {Version}", version);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to show update notification");
        }
    }

    public void ShowDownloadCompleteNotification(string fileName)
    {
        try
        {
            using var process = Process.Start(new ProcessStartInfo
            {
                FileName = "notify-send",
                Arguments = $"--urgency=low --icon=dialog-information \"Download Complete\" \"{fileName}",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            });

            if (process != null)
            {
                process.WaitForExit(5000);
            }

            _logger.LogInformation("Download complete notification shown for file {File}", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to show download notification");
        }
    }

    public void ClearAllNotifications()
    {
        try
        {
            using var process = Process.Start(new ProcessStartInfo
            {
                FileName = "notify-send",
                Arguments = "--expire-time=1 \"\" \"\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            });

            if (process != null)
            {
                process.WaitForExit(3000);
            }

            _logger.LogDebug("Notifications cleared");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to clear notifications");
        }
    }
}
