using PCL.CE.Neo.Core.Abstractions;

namespace PCL.CE.Neo.Platform.Linux;

public class LinuxDialogService : IDialogService
{
    public string? ShowOpenFileDialog(string filter, string? initialDirectory = null)
    {
        try
        {
            var process = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "zenity",
                    Arguments = "--file-selection",
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
                    FileName = "zenity",
                    Arguments = $"--file-selection --save --filename=\"{defaultFileName}\"",
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
                    FileName = "zenity",
                    Arguments = "--file-selection --directory",
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
        var buttonType = buttons switch
        {
            DialogButtons.OK => "--info",
            DialogButtons.OKCancel => "--warning",
            DialogButtons.YesNo => "--question",
            DialogButtons.YesNoCancel => "--question",
            _ => "--info"
        };

        var buttonText = buttons switch
        {
            DialogButtons.OK => "--ok-label=OK",
            DialogButtons.OKCancel => "--ok-label=OK --cancel-label=Cancel",
            DialogButtons.YesNo => "--ok-label=Yes --cancel-label=No",
            DialogButtons.YesNoCancel => "--ok-label=Yes --cancel-label=No",
            _ => ""
        };

        try
        {
            var process = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "zenity",
                    Arguments = $"{buttonType} --text=\"{message}\" --title=\"{title}\" {buttonText}",
                    RedirectStandardOutput = true,
                    UseShellExecute = false
                }
            };
            process.Start();
            var result = process.StandardOutput.ReadToEnd().Trim();
            process.WaitForExit();

            if (process.ExitCode == 0)
            {
                return buttons == DialogButtons.YesNo || buttons == DialogButtons.YesNoCancel
                    ? DialogResult.Yes
                    : DialogResult.OK;
            }
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
