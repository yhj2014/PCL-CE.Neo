namespace PCL_CE.Neo.Platform.Linux;

public class LinuxNotificationService : INotificationService
{
    public void ShowNotification(NotificationInfo notification)
    {
        try
        {
            var urgency = notification.Type switch
            {
                NotificationType.Info => "low",
                NotificationType.Success => "normal",
                NotificationType.Warning => "critical",
                NotificationType.Error => "critical",
                _ => "normal"
            };

            var process = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "notify-send",
                    Arguments = $"--urgency={urgency} \"{notification.Title}\" \"{notification.Message}\"",
                    UseShellExecute = false
                }
            };
            process.Start();
        }
        catch
        {
            // Fallback - no notification
        }
    }

    public void ShowUpdateNotification(string version, string releaseNotes)
    {
        ShowNotification(new NotificationInfo
        {
            Title = "PCL 更新通知",
            Message = $"新版本 {version} 已可用！\n\n{releaseNotes}",
            Type = NotificationType.Info
        });
    }

    public void ShowDownloadCompleteNotification(string fileName)
    {
        ShowNotification(new NotificationInfo
        {
            Title = "下载完成",
            Message = $"下载完成：{fileName}",
            Type = NotificationType.Success
        });
    }

    public void ClearAllNotifications()
    {
        // notify-send doesn't support clearing notifications
    }
}
