namespace PCL_CE.Neo.UI.Services;

public class NotificationService : Core.Abstractions.INotificationService
{
    public List<Core.Abstractions.NotificationInfo> Notifications { get; private set; } = new List<Core.Abstractions.NotificationInfo>();

    public void ShowNotification(Core.Abstractions.NotificationInfo notification)
    {
        Notifications.Add(notification);
#if WINDOWS || MACCATALYST || LINUX
        ShowPlatformNotification(notification);
#endif
    }

    private void ShowPlatformNotification(Core.Abstractions.NotificationInfo notification)
    {
#if WINDOWS
        ShowWindowsToastNotification(notification);
#elif MACCATALYST
        ShowMacOSNotification(notification);
#elif LINUX
        ShowLinuxNotification(notification);
#endif
    }

#if WINDOWS
    private void ShowWindowsToastNotification(Core.Abstractions.NotificationInfo notification)
    {
        try
        {
            var template = Windows.UI.Notifications.ToastNotificationManager.GetTemplateContent(
                Windows.UI.Notifications.ToastTemplateType.ToastText02);
            var textNodes = template.GetElementsByTagName("text");
            if (textNodes.Length >= 2)
            {
                textNodes[0].AppendChild(template.CreateTextNode(notification.Title));
                textNodes[1].AppendChild(template.CreateTextNode(notification.Message));
            }

            var notifier = Windows.UI.Notifications.ToastNotificationManager.CreateToastNotifier();
            var toast = new Windows.UI.Notifications.ToastNotification(template);
            notifier.Show(toast);
        }
        catch
        {
        }
    }
#endif

#if MACCATALYST
    private void ShowMacOSNotification(Core.Abstractions.NotificationInfo notification)
    {
        try
        {
            var escapedTitle = notification.Title.Replace("\"", "\\\"");
            var escapedMessage = notification.Message.Replace("\"", "\\\"");
            var script = $"-e 'display notification \"{escapedMessage}\" with title \"{escapedTitle}\"'";

            var process = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "osascript",
                    Arguments = script,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }
            };
            process.Start();
            process.WaitForExit(5000);
        }
        catch
        {
        }
    }
#endif

#if LINUX
    private void ShowLinuxNotification(Core.Abstractions.NotificationInfo notification)
    {
        try
        {
            var escapedTitle = notification.Title.Replace("\"", "\\\"");
            var escapedMessage = notification.Message.Replace("\"", "\\\"");
            var urgency = notification.Type switch
            {
                Core.Abstractions.NotificationType.Error => "critical",
                Core.Abstractions.NotificationType.Warning => "normal",
                _ => "low"
            };

            var process = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "notify-send",
                    Arguments = $"--urgency={urgency} \"{escapedTitle}\" \"{escapedMessage}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }
            };
            process.Start();
            process.WaitForExit(5000);
        }
        catch
        {
        }
    }
#endif

    public void ShowUpdateNotification(string version, string notes)
    {
        ShowNotification(new Core.Abstractions.NotificationInfo
        {
            Title = $"Update {version}",
            Message = notes,
            Type = Core.Abstractions.NotificationType.Info
        });
    }

    public void ShowDownloadCompleteNotification(string fileName)
    {
        ShowNotification(new Core.Abstractions.NotificationInfo
        {
            Title = "Download Complete",
            Message = $"File '{fileName}' downloaded successfully",
            Type = Core.Abstractions.NotificationType.Success
        });
    }

    public void ClearAllNotifications()
    {
        Notifications.Clear();
#if WINDOWS
        try
        {
            Windows.UI.Notifications.ToastNotificationManager.History.Clear();
        }
        catch
        {
        }
#endif
    }
}
