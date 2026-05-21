namespace PCL_CE.Neo.UI.Services;

public class NotificationService : Core.Abstractions.INotificationService
{
    public List<Core.Abstractions.NotificationInfo> Notifications { get; private set; } = new List<Core.Abstractions.NotificationInfo>();

    public void ShowNotification(Core.Abstractions.NotificationInfo notification)
    {
#if WINDOWS || MACCATALYST || LINUX
        Notifications.Add(notification);
        // TODO: Implement using platform-specific notification APIs
        // Windows: ToastNotificationManager
        // macOS: UNUserNotificationCenter
        // Linux: libnotify (notify-send)
#else
        throw new PlatformNotSupportedException("NotificationService requires Uno Platform");
#endif
    }

    public void ShowUpdateNotification(string version, string notes)
    {
#if WINDOWS || MACCATALYST || LINUX
        ShowNotification(new Core.Abstractions.NotificationInfo
        {
            Title = $"Update {version}",
            Message = notes,
            Type = Core.Abstractions.NotificationType.Info
        });
#else
        throw new PlatformNotSupportedException("NotificationService requires Uno Platform");
#endif
    }

    public void ShowDownloadCompleteNotification(string fileName)
    {
#if WINDOWS || MACCATALYST || LINUX
        ShowNotification(new Core.Abstractions.NotificationInfo
        {
            Title = "Download Complete",
            Message = $"File '{fileName}' downloaded successfully",
            Type = Core.Abstractions.NotificationType.Success
        });
#else
        throw new PlatformNotSupportedException("NotificationService requires Uno Platform");
#endif
    }

    public void ClearAllNotifications()
    {
#if WINDOWS || MACCATALYST || LINUX
        Notifications.Clear();
#else
        throw new PlatformNotSupportedException("NotificationService requires Uno Platform");
#endif
    }
}
