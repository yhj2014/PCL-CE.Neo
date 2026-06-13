using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Logging;
using PCL_CE.Neo.Core.Abstractions;

namespace PCL_CE.Neo.Platform.Windows;

public class WindowsNotificationService : INotificationService
{
    private readonly ILogger<WindowsNotificationService> _logger;
    private int _notificationCount;

    public WindowsNotificationService(ILogger<WindowsNotificationService> logger)
    {
        _logger = logger;
        try
        {
            _logger.LogDebug("正在初始化 Windows 通知服务");
            _notificationCount = 0;
            _logger.LogInformation("Windows 通知服务初始化完成");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "初始化 Windows 通知服务时发生错误");
        }
    }

    public void ShowNotification(NotificationInfo notification)
    {
        try
        {
            if (notification == null)
            {
                _logger.LogWarning("尝试显示空通知，已忽略");
                return;
            }

            _logger.LogDebug("显示通知，标题: {Title}, 类型: {Type}", notification.Title, notification.Type);
            var iconMapping = notification.Type switch
            {
                NotificationType.Info => "'i'",
                NotificationType.Success => "'ok'",
                NotificationType.Warning => "'warn'",
                NotificationType.Error => "'err'",
                _ => "'i'"
            };

            var title = string.IsNullOrEmpty(notification.Title) ? "通知" : notification.Title;
            var message = string.IsNullOrEmpty(notification.Message) ? string.Empty : notification.Message;

            var script = $@"
Add-Type -AssemblyName System.Windows.Forms
$title = '{title.Replace("'", "''")}'
$message = '{message.Replace("'", "''")}'
$icon = [System.Windows.Forms.MessageBoxIcon]::{GetIconName(notification.Type)}
[System.Windows.Forms.MessageBox]::Show($message, $title, [System.Windows.Forms.MessageBoxButtons]::OK, $icon) | Out-Null
Write-Output 'shown'
";

            var startInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-Command \"{script}\"",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using var process = Process.Start(startInfo);
            process?.WaitForExit(60000);
            _notificationCount++;
            _logger.LogInformation("通知 #{Count} 已显示，类型: {Type}", _notificationCount, notification.Type);

            if (notification.Action != null)
            {
                _logger.LogDebug("执行通知关联的操作");
                notification.Action.Invoke();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "显示通知时发生错误");
        }
    }

    public void ShowUpdateNotification(string version, string releaseNotes)
    {
        try
        {
            _logger.LogDebug("显示更新通知，版本: {Version}", version);
            var notification = new NotificationInfo
            {
                Title = "软件更新",
                Message = $"新版本 {version} 已可用\n\n{releaseNotes}",
                Type = NotificationType.Info,
                ActionText = "了解更多",
                Action = null
            };
            ShowNotification(notification);
            _logger.LogInformation("更新通知已显示，版本: {Version}", version);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "显示更新通知时发生错误");
        }
    }

    public void ShowDownloadCompleteNotification(string fileName)
    {
        try
        {
            _logger.LogDebug("显示下载完成通知，文件: {FileName}", fileName);
            var notification = new NotificationInfo
            {
                Title = "下载完成",
                Message = $"文件 '{fileName}' 已成功下载",
                Type = NotificationType.Success,
                ActionText = "打开文件夹",
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
                                    FileName = directory,
                                    UseShellExecute = true
                                });
                            }
                        }
                    }
                    catch (Exception innerEx)
                    {
                        _logger.LogWarning(innerEx, "打开下载文件夹时发生错误");
                    }
                }
            };
            ShowNotification(notification);
            _logger.LogInformation("下载完成通知已显示，文件: {FileName}", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "显示下载完成通知时发生错误");
        }
    }

    public void ClearAllNotifications()
    {
        try
        {
            _logger.LogDebug("清除所有通知");
            _notificationCount = 0;
            _logger.LogInformation("已清除所有通知");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "清除通知时发生错误");
        }
    }

    private static string GetIconName(NotificationType type)
    {
        return type switch
        {
            NotificationType.Info => "Information",
            NotificationType.Success => "Information",
            NotificationType.Warning => "Warning",
            NotificationType.Error => "Error",
            _ => "Information"
        };
    }
}
