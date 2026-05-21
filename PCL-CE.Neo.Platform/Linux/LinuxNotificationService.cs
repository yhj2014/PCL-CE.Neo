using PCL_CE.Neo.Core.Abstractions;

namespace PCL_CE.Neo.Platform.Linux;

public class LinuxNotificationService : INotificationService
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
            Title = "PCL Update",
            Message = $"New version {version} is available!\n\n{notes}",
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
