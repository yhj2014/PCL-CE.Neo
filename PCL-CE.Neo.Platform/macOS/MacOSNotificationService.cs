using PCL_CE.Neo.Core.Abstractions;

namespace PCL_CE.Neo.Platform.macOS;

public class MacOSNotificationService : INotificationService
{
    public List<NotificationInfo> Notifications { get; private set; } = new List<NotificationInfo>();

    public void ShowNotification(NotificationInfo notification)
    {
        Notifications.Add(notification);
    }

    public void ShowUpdateNotification(string version, string notes)
    {
        Notifications.Add(new NotificationInfo
        {
            Title = $"Update {version}",
            Message = notes,
            Type = NotificationType.Info
        });
    }

    public void ShowDownloadCompleteNotification(string fileName)
    {
        Notifications.Add(new NotificationInfo
        {
            Title = "Download Complete",
            Message = $"File '{fileName}' downloaded successfully",
            Type = NotificationType.Success
        });
    }

    public void ClearAllNotifications()
    {
        Notifications.Clear();
    }
}
