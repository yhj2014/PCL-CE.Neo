namespace PCL_CE.Neo.Platform.Windows;

public class WindowsNotificationService : INotificationService
{
    public void ShowNotification(NotificationInfo notification)
    {
        // Simple implementation - show a message box
        // For full Windows toast notifications, we'd need Windows Runtime APIs
        System.Windows.MessageBox.Show(
            notification.Message,
            notification.Title,
            System.Windows.MessageBoxButton.OK,
            GetMessageBoxIcon(notification.Type)
        );
    }

    public void ShowUpdateNotification(string version, string releaseNotes)
    {
        System.Windows.MessageBox.Show(
            $"新版本 {version} 已可用！\n\n{releaseNotes}",
            "PCL 更新通知",
            System.Windows.MessageBoxButton.OK,
            System.Windows.MessageBoxImage.Information
        );
    }

    public void ShowDownloadCompleteNotification(string fileName)
    {
        System.Windows.MessageBox.Show(
            $"下载完成：{fileName}",
            "下载完成",
            System.Windows.MessageBoxButton.OK,
            System.Windows.MessageBoxImage.Information
        );
    }

    public void ClearAllNotifications()
    {
        // No-op for this simple implementation
    }

    private System.Windows.MessageBoxImage GetMessageBoxIcon(NotificationType type)
    {
        return type switch
        {
            NotificationType.Info => System.Windows.MessageBoxImage.Information,
            NotificationType.Success => System.Windows.MessageBoxImage.Information,
            NotificationType.Warning => System.Windows.MessageBoxImage.Warning,
            NotificationType.Error => System.Windows.MessageBoxImage.Error,
            _ => System.Windows.MessageBoxImage.Information
        };
    }
}
