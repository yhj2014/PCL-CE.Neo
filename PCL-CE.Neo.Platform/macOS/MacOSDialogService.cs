using PCL_CE.Neo.Core.Abstractions;

namespace PCL_CE.Neo.Platform.macOS;

public class MacOSDialogService : IDialogService
{
    public string? ShowOpenFileDialog(string filter, string? initialDirectory = null)
    {
        try
        {
            var process = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "osascript",
                    Arguments = "-e 'POSIX path of (choose file)'",
                    RedirectStandardOutput = true,
                    UseShellExecute = false
                }
            };
            process.Start();
            var result = process.StandardOutput.ReadToEnd().Trim();
            process.WaitForExit();
            return string.IsNullOrEmpty(result) ? null : result;
        }
        catch
        {
            return null;
        }
    }

    public string? ShowSaveFileDialog(string filter, string defaultFileName, string? initialDirectory = null)
    {
        try
        {
            var process = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "osascript",
                    Arguments = "-e 'POSIX path of (choose file name default location \"' + defaultFileName + '\")'",
                    RedirectStandardOutput = true,
                    UseShellExecute = false
                }
            };
            process.Start();
            var result = process.StandardOutput.ReadToEnd().Trim();
            process.WaitForExit();
            return string.IsNullOrEmpty(result) ? null : result;
        }
        catch
        {
            return null;
        }
    }

    public string? ShowOpenFolderDialog(string? initialDirectory = null)
    {
        try
        {
            var process = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "osascript",
                    Arguments = "-e 'POSIX path of (choose folder)'",
                    RedirectStandardOutput = true,
                    UseShellExecute = false
                }
            };
            process.Start();
            var result = process.StandardOutput.ReadToEnd().Trim();
            process.WaitForExit();
            return string.IsNullOrEmpty(result) ? null : result;
        }
        catch
        {
            return null;
        }
    }

    public DialogResult ShowMessageBox(string message, string title, DialogButtons buttons)
    {
        var buttonScript = buttons switch
        {
            DialogButtons.OK => "\"OK\"",
            DialogButtons.OKCancel => "\"OK\" buttons {\"OK\", \"Cancel\"}",
            DialogButtons.YesNo => "\"Yes\" buttons {\"Yes\", \"No\"}",
            DialogButtons.YesNoCancel => "\"Yes\" buttons {\"Yes\", \"No\", \"Cancel\"}",
            _ => "\"OK\""
        };

        try
        {
            var script = $"-e 'display dialog \"{message.Replace("\"", "\\\"")}\" with title \"{title.Replace("\"", "\\\"")}\" buttons {buttonScript}'";
            var process = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "osascript",
                    Arguments = script,
                    RedirectStandardOutput = true,
                    UseShellExecute = false
                }
            };
            process.Start();
            var result = process.StandardOutput.ReadToEnd().Trim();
            process.WaitForExit();

            if (result.Contains("OK") || result.Contains("Yes"))
                return DialogResult.OK;
            if (result.Contains("No"))
                return DialogResult.No;
            return DialogResult.Cancel;
        }
        catch
        {
            return DialogResult.None;
        }
    }

    public bool ShowConfirmation(string message, string title)
    {
        return ShowMessageBox(message, title, DialogButtons.YesNo) == DialogResult.Yes;
    }
}
