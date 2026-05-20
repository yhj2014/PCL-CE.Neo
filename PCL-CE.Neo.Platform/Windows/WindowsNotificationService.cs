using PCL_CE.Neo.Core.Abstractions;

namespace PCL_CE.Neo.Platform.Windows;

public class WindowsNotificationService : INotificationService
{
    public void ShowNotification(NotificationInfo notification)
    {
        Console.WriteLine($"[{notification.Title}: {notification.Message}");
    }

    public void ShowUpdateNotification(string version, string releaseNotes)
    {
        Console.WriteLine($"新版本 {version} 已可用！\n\n{releaseNotes}");
    }

    public void ShowDownloadCompleteNotification(string fileName)
    {
        Console.WriteLine($"下载完成：{fileName}");
    }

    public void ClearAllNotifications()
    {
    }
}
