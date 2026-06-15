using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Logging;
using PCL_CE.Neo.Core.Abstractions;

namespace PCL_CE.Neo.Platform.macOS;

public class MacOSNotificationService : INotificationService
{
    private readonly ILogger<MacOSNotificationService> _logger;
    private int _notificationCount;

    public MacOSNotificationService() : this(Microsoft.Extensions.Logging.Abstractions.NullLogger<MacOSNotificationService>.Instance)
    {
    }

    public MacOSNotificationService(ILogger<MacOSNotificationService> logger)
    {
        _logger = logger;
        try
        {
            _logger.LogDebug("Initializing macOS notification service");
            _notificationCount = 0;
            _logger.LogInformation("macOS notification service initialized");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing macOS notification service");
        }
    }

    public void ShowNotification(NotificationInfo notification)
    {
        try
        {
            if (notification == null)
            {
                _logger.LogWarning("Attempted to show null notification, ignored");
                return;
            }

            _logger.LogDebug("Showing notification, title: {Title}, type: {Type}", notification.Title, notification.Type);

            var title = string.IsNullOrEmpty(notification.Title) ? "Notification" : notification.Title;
            var message = string.IsNullOrEmpty(notification.Message) ? string.Empty : notification.Message;

            var soundName = notification.Type switch
            {
                NotificationType.Info => "Glass",
                NotificationType.Success => "Glass",
                NotificationType.Warning => "Basso",
                NotificationType.Error => "Basso",
                _ => "Glass"
            };

            var escapedTitle = title.Replace("\\", "\\\\").Replace("\"", "\\\"");
            var escapedMessage = message.Replace("\\", "\\\\").Replace("\"", "\\\"");

            var script = $"display notification \"{escapedMessage}\" with title \"{escapedTitle}\" sound name \"{soundName}\"";

            var startInfo = new ProcessStartInfo
            {
                FileName = "osascript",
                Arguments = $"-e \"{script}\"",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using var process = Process.Start(startInfo);
            process?.WaitForExit(10000);
            _notificationCount++;
            _logger.LogInformation("Notification #{Count} shown, type: {Type}", _notificationCount, notification.Type);

            if (notification.Action != null)
            {
                _logger.LogDebug("Executing notification associated action");
                notification.Action.Invoke();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error showing notification");
        }
    }

    public void ShowUpdateNotification(string version, string releaseNotes)
    {
        try
        {
            _logger.LogDebug("Showing update notification, version: {Version}", version);
            var notification = new NotificationInfo
            {
                Title = "Software Update",
                Message = $"New version {version} is now available\n\n{releaseNotes}",
                Type = NotificationType.Info,
                ActionText = "Learn More",
                Action = null
            };
            ShowNotification(notification);
            _logger.LogInformation("Update notification shown, version: {Version}", version);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error showing update notification");
        }
    }

    public void ShowDownloadCompleteNotification(string fileName)
    {
        try
        {
            _logger.LogDebug("Showing download complete notification, file: {FileName}", fileName);
            var notification = new NotificationInfo
            {
                Title = "Download Complete",
                Message = $"File \"{fileName}\" has been downloaded successfully",
                Type = NotificationType.Success,
                ActionText = "Open Folder",
                Action = () =>
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(fileName) && File.Exists(fileName))
                        {
                            var directory = Path.GetDirectoryName(fileName);
                            if (!string.IsNullOrEmpty(directory) && Directory.Exists(directory))
                            {
                                Process.Start(new ProcessStartInfo
                                {
                                    FileName = "open",
                                    Arguments = "\"" + directory.Replace("\"", "\\\"") + "\"",
                                    UseShellExecute = false,
                                    CreateNoWindow = true
                                });
                            }
                        }
                    }
                    catch (Exception innerEx)
                    {
                        _logger.LogWarning(innerEx, "Error opening download folder");
                    }
                }
            };
            ShowNotification(notification);
            _logger.LogInformation("Download complete notification shown, file: {FileName}", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error showing download complete notification");
        }
    }

    public void ClearAllNotifications()
    {
        try
        {
            _logger.LogDebug("Clearing all notifications");
            var script = "tell application \"System Events\" to tell process \"NotificationCenter\"\ntry\n    repeat\n        try\n            tell button 1 of UI element 1 of scroll area 1 of window 1 to perform action \"AXPress\"\n        on error\n            exit repeat\n        end try\n    end repeat\nend try\nend tell";

            var startInfo = new ProcessStartInfo
            {
                FileName = "osascript",
                Arguments = $"-e \"{script.Replace("\"", "\\\"")}\"",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using var process = Process.Start(startInfo);
            process?.WaitForExit(5000);
            _notificationCount = 0;
            _logger.LogInformation("All notifications cleared");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing notifications");
        }
    }
}
