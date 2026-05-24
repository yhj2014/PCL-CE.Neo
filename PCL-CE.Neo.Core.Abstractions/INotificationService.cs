namespace PCL_CE.Neo.Core.Abstractions;

public interface INotificationService
{
    void ShowNotification(NotificationInfo notification);
    void ShowUpdateNotification(string version, string releaseNotes);
    void ShowDownloadCompleteNotification(string fileName);
    void ClearAllNotifications();
}

public class NotificationInfo
{
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public string? ActionText { get; set; }
    public Action? Action { get; set; }
}

public enum NotificationType
{
    Info,
    Success,
    Warning,
    Error
}
