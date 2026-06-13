using System.Diagnostics;
using Microsoft.Extensions.Logging;
using PCL_CE.Neo.Core.Abstractions;

namespace PCL_CE.Neo.Platform.Linux;

public class LinuxDialogService : IDialogService
{
    private readonly ILogger<LinuxDialogService> _logger;

    public LinuxDialogService() : this(Microsoft.Extensions.Logging.Abstractions.NullLogger<LinuxDialogService>.Instance)
    {
    }

    public LinuxDialogService(ILogger<LinuxDialogService> logger)
    {
        _logger = logger;
        try
        {
            _logger.LogDebug("LinuxDialogService initializing");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error during LinuxDialogService initialization");
        }
    }

    private string? FindDialogTool()
    {
        var tools = new[] { "zenity", "kdialog" };
        foreach (var tool in tools)
        {
            try
            {
                using var check = Process.Start(new ProcessStartInfo
                {
                    FileName = "which",
                    Arguments = tool,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                });

                if (check != null)
                {
                    var output = check.StandardOutput.ReadToEnd().Trim();
                    check.WaitForExit(1000);
                    if (!string.IsNullOrWhiteSpace(output))
                    {
                        return tool;
                    }
                }
            }
            catch { }
        }

        return null;
    }

    public string? ShowOpenFileDialog(string filter, string? initialDirectory = null)
    {
        try
        {
            var tool = FindDialogTool();
            if (string.IsNullOrEmpty(tool))
            {
                _logger.LogWarning("No dialog tool available (zenity/kdialog not found)");
                return null;
            }

            var args = tool == "zenity"
                ? $"--file-selection --title=\"Select File\" {(string.IsNullOrEmpty(initialDirectory) ? "" : $"--filename=\"{initialDirectory}\"")}"
                : $"--getopenfilename {(string.IsNullOrEmpty(initialDirectory) ? "" : initialDirectory)}";

            using var process = Process.Start(new ProcessStartInfo
            {
                FileName = tool,
                Arguments = args,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            });

            if (process != null)
            {
                var output = process.StandardOutput.ReadToEnd().Trim();
                process.WaitForExit(60000);

                if (process.ExitCode == 0 && !string.IsNullOrEmpty(output))
                {
                    _logger.LogDebug("Open file dialog result: {Path}", output);
                    return output;
                }
            }

            _logger.LogDebug("User canceled open file dialog");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to show open file dialog");
            return null;
        }
    }

    public string? ShowSaveFileDialog(string filter, string defaultFileName, string? initialDirectory = null)
    {
        try
        {
            var tool = FindDialogTool();
            if (string.IsNullOrEmpty(tool))
            {
                _logger.LogWarning("No dialog tool available");
                return null;
            }

            var startFile = Path.Combine(initialDirectory ?? Directory.GetCurrentDirectory(), defaultFileName);

            var args = tool == "zenity"
                ? $"--file-selection --save --confirm-overwrite --filename=\"{startFile}\""
                : $"--getsavefilename \"{startFile}\"";

            using var process = Process.Start(new ProcessStartInfo
            {
                FileName = tool,
                Arguments = args,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            });

            if (process != null)
            {
                var output = process.StandardOutput.ReadToEnd().Trim();
                process.WaitForExit(60000);

                if (process.ExitCode == 0 && !string.IsNullOrEmpty(output))
                {
                    _logger.LogDebug("Save file dialog result: {Path}", output);
                    return output;
                }
            }

            _logger.LogDebug("User canceled save file dialog");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to show save file dialog");
            return null;
        }
    }

    public string? ShowOpenFolderDialog(string? initialDirectory = null)
    {
        try
        {
            var tool = FindDialogTool();
            if (string.IsNullOrEmpty(tool))
            {
                _logger.LogWarning("No dialog tool available");
                return null;
            }

            var args = tool == "zenity"
                ? $"--file-selection --directory {(string.IsNullOrEmpty(initialDirectory) ? "" : $"--filename=\"{initialDirectory}\"")}"
                : $"--getexistingdirectory {(string.IsNullOrEmpty(initialDirectory) ? "" : initialDirectory)}";

            using var process = Process.Start(new ProcessStartInfo
            {
                FileName = tool,
                Arguments = args,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            });

            if (process != null)
            {
                var output = process.StandardOutput.ReadToEnd().Trim();
                process.WaitForExit(60000);

                if (process.ExitCode == 0 && !string.IsNullOrEmpty(output))
                {
                    _logger.LogDebug("Open folder dialog result: {Path}", output);
                    return output;
                }
            }

            _logger.LogDebug("User canceled open folder dialog");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to show open folder dialog");
            return null;
        }
    }

    public DialogResult ShowMessageBox(string message, string title, DialogButtons buttons)
    {
        try
        {
            var tool = FindDialogTool();
            if (string.IsNullOrEmpty(tool))
            {
                _logger.LogWarning("No dialog tool available, treating as OK");
                return DialogResult.OK;
            }

            var args = "";
            var isQuestion = false;

            switch (buttons)
            {
                case DialogButtons.OK:
                    args = tool == "zenity"
                        ? $"--info --title=\"{title}\" --text=\"{message}\""
                        : $"--msgbox \"{message}\" --title \"{title}\"";
                    break;
                case DialogButtons.OKCancel:
                case DialogButtons.YesNoCancel:
                case DialogButtons.YesNo:
                    args = tool == "zenity"
                        ? $"--question --title=\"{title}\" --text=\"{message}\""
                        : $"--question \"{message}\" --title \"{title}\"";
                    isQuestion = true;
                    break;
            }

            using var process = Process.Start(new ProcessStartInfo
            {
                FileName = tool,
                Arguments = args,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            });

            if (process != null)
            {
                process.WaitForExit(60000);

                if (isQuestion)
                {
                    return process.ExitCode == 0 ? DialogResult.Yes : DialogResult.No;
                }

                return process.ExitCode == 0 ? DialogResult.OK : DialogResult.Cancel;
            }

            return DialogResult.Cancel;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to show message box");
            return DialogResult.OK;
        }
    }

    public bool ShowConfirmation(string message, string title)
    {
        try
        {
            var tool = FindDialogTool();
            if (string.IsNullOrEmpty(tool))
            {
                _logger.LogWarning("No dialog tool available, treating as not confirmed");
                return false;
            }

            var args = tool == "zenity"
                ? $"--question --title=\"{title}\" --text=\"{message}\""
                : $"--question \"{message}\" --title \"{title}\"";

            using var process = Process.Start(new ProcessStartInfo
            {
                FileName = tool,
                Arguments = args,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            });

            if (process != null)
            {
                process.WaitForExit(60000);
                var confirmed = process.ExitCode == 0;
                _logger.LogDebug("Confirmation dialog result: {Confirmed}", confirmed);
                return confirmed;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to show confirmation dialog");
            return false;
        }
    }
}
