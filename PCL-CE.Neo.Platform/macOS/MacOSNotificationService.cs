namespace PCL_CE.Neo.Platform.macOS;

public class MacOSNotificationService : Core.Abstractions.INotificationService
{
#if MACCATALYST
    public List<Core.Abstractions.NotificationInfo> Notifications { get; private set; } = new List<Core.Abstractions.NotificationInfo>();

    public void ShowNotification(Core.Abstractions.NotificationInfo notification)
    {
        Notifications.Add(notification);
    }

    public void ShowUpdateNotification(string version, string notes)
    {
        Notifications.Add(new Core.Abstractions.NotificationInfo
        {
            Title = $"Update {version}",
            Message = notes,
            Type = Core.Abstractions.NotificationType.Info
        });
    }

    public void ShowDownloadCompleteNotification(string fileName)
    {
        Notifications.Add(new Core.Abstractions.NotificationInfo
        {
            Title = "Download Complete",
            Message = $"File '{fileName}' downloaded successfully",
            Type = Core.Abstractions.NotificationType.Success
        });
    }

    public void ClearAllNotifications()
    {
        Notifications.Clear();
    }
#else
    public void ShowNotification(Core.Abstractions.NotificationInfo notification)
    {
        throw new PlatformNotSupportedException("此功能在 macOS 上尚未实现");
    }

    public void ShowUpdateNotification(string version, string notes)
    {
        throw new PlatformNotSupportedException("此功能在 macOS 上尚未实现");
    }

    public void ShowDownloadCompleteNotification(string fileName)
    {
        throw new PlatformNotSupportedException("此功能在 macOS 上尚未实现");
    }

    public void ClearAllNotifications()
    {
        throw new PlatformNotSupportedException("此功能在 macOS 上尚未实现");
    }
#endif
}
