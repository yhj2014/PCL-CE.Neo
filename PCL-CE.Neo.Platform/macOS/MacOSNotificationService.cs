using PCL_CE.Neo.Core.Abstractions;

namespace PCL_CE.Neo.Platform.macOS;

public class MacOSNotificationService : INotificationService
{
    public void ShowNotification(string title, string message, NotificationType type = NotificationType.Info)
    {
        try
        {
            var script = $"-e 'display notification \"{message.Replace("\"", "\\\"")}\" with title \"{title.Replace("\"", "\\\"")}\"'";
            var process = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "osascript",
                    Arguments = script,
                    UseShellExecute = false
                }
            };
            process.Start();
        }
        catch
        {
        }
    }

    public void ShowToast(string title, string message, int durationMs = 3000)
    {
        ShowNotification(title, message);
    }
}
