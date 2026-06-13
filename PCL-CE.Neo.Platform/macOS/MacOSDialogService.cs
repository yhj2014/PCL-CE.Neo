using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Logging;
using PCL_CE.Neo.Core.Abstractions;

namespace PCL_CE.Neo.Platform.macOS;

public class MacOSDialogService : IDialogService
{
    private readonly ILogger<MacOSDialogService> _logger;

    public MacOSDialogService() : this(Microsoft.Extensions.Logging.Abstractions.NullLogger<MacOSDialogService>.Instance) { }

    public MacOSDialogService(ILogger<MacOSDialogService> logger)
    {
        _logger = logger;
        try
        {
            _logger.LogDebug("Initializing macOS dialog service");
            _logger.LogInformation("macOS dialog service initialized");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing macOS dialog service");
        }
    }

    private static string EscapeAppleScript(string input)
    {
        return (input ?? string.Empty).Replace("\\", "\\\\").Replace("\"", "\\\"");
    }

    public string? ShowOpenFileDialog(string filter, string? initialDirectory = null)
    {
        try
        {
            _logger.LogDebug("Showing open file dialog, filter: {Filter}, initial directory: {Dir}",
                filter, initialDirectory ?? "(default)");

            var escapedDir = string.IsNullOrEmpty(initialDirectory)
                ? "path to desktop folder"
                : "POSIX file \"" + EscapeAppleScript(initialDirectory) + "\"";

            var filterPart = string.IsNullOrEmpty(filter)
                ? string.Empty
                : " of type {\"" + string.Join("\",\"", EscapeAppleScript(filter)
                    .Split(new[] { '|', ';' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim())) + "\"}";

            var script = "set theResult to choose file" + filterPart + " with prompt \"Open File\" default location " + escapedDir;

            var startInfo = new ProcessStartInfo
            {
                FileName = "osascript",
                Arguments = $"-e \"{EscapeAppleScript(script)}\"",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = Encoding.UTF8
            };

            using var process = Process.Start(startInfo);
            if (process != null)
            {
                var output = process.StandardOutput.ReadToEnd().Trim();
                process.WaitForExit(60000);

                if (process.ExitCode == 0 && !string.IsNullOrWhiteSpace(output))
                {
                    // osascript may return AppleScript alias path, try to convert to POSIX path
                    var filePath = output.StartsWith("alias ") ? output.Substring(6).Trim() : output;
                    if (filePath.StartsWith("\"") && filePath.EndsWith("\""))
                    {
                        filePath = filePath.Substring(1, filePath.Length - 2);
                    }

                    if (!filePath.StartsWith("/") && filePath.Contains(":"))
                    {
                        // HFS path to POSIX
                        try
                        {
                            var posixProcess = Process.Start(new ProcessStartInfo
                            {
                                FileName = "osascript",
                                Arguments = $"-e \"POSIX path of alias \\\"{EscapeAppleScript(filePath)}\\\"\"",
                                UseShellExecute = false,
                                CreateNoWindow = true,
                                RedirectStandardOutput = true
                            });
                            if (posixProcess != null)
                            {
                                var posixOutput = posixProcess.StandardOutput.ReadToEnd().Trim();
                                posixProcess.WaitForExit(3000);
                                if (!string.IsNullOrWhiteSpace(posixOutput))
                                {
                                    filePath = posixOutput;
                                }
                            }
                        }
                        catch { }
                    }

                    _logger.LogInformation("User selected file: {FilePath}", filePath);
                    return filePath;
                }
                else
                {
                    _logger.LogInformation("User canceled file dialog or selection invalid, exit code: {Code}", process.ExitCode);
                    return null;
                }
            }

            _logger.LogWarning("Failed to start file dialog process");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error showing open file dialog");
            return null;
        }
    }

    public string? ShowSaveFileDialog(string filter, string defaultFileName, string? initialDirectory = null)
    {
        try
        {
            _logger.LogDebug("Showing save file dialog, filter: {Filter}, default name: {Name}, initial directory: {Dir}",
                filter, defaultFileName, initialDirectory ?? "(default)");

            var escapedDir = string.IsNullOrEmpty(initialDirectory)
                ? "path to desktop folder"
                : "POSIX file \"" + EscapeAppleScript(initialDirectory) + "\"";

            var escapedName = EscapeAppleScript(defaultFileName ?? string.Empty);
            var script = $"set theResult to choose file name with prompt \"Save File\" default name \"{escapedName}\" default location {escapedDir}";

            var startInfo = new ProcessStartInfo
            {
                FileName = "osascript",
                Arguments = $"-e \"{EscapeAppleScript(script)}\"",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = Encoding.UTF8
            };

            using var process = Process.Start(startInfo);
            if (process != null)
            {
                var output = process.StandardOutput.ReadToEnd().Trim();
                process.WaitForExit(60000);

                if (process.ExitCode == 0 && !string.IsNullOrWhiteSpace(output))
                {
                    var filePath = output.StartsWith("alias ") ? output.Substring(6).Trim() : output;
                    if (filePath.StartsWith("\"") && filePath.EndsWith("\""))
                    {
                        filePath = filePath.Substring(1, filePath.Length - 2);
                    }
                    _logger.LogInformation("User saving file: {FilePath}", filePath);
                    return filePath;
                }
                else
                {
                    _logger.LogInformation("User canceled save file dialog");
                    return null;
                }
            }

            _logger.LogWarning("Failed to start save file dialog process");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error showing save file dialog");
            return null;
        }
    }

    public string? ShowOpenFolderDialog(string? initialDirectory = null)
    {
        try
        {
            _logger.LogDebug("Showing open folder dialog, initial directory: {Dir}", initialDirectory ?? "(default)");

            var escapedDir = string.IsNullOrEmpty(initialDirectory)
                ? "path to desktop folder"
                : "POSIX file \"" + EscapeAppleScript(initialDirectory) + "\"";

            var script = "set theResult to choose folder with prompt \"Open Folder\" default location " + escapedDir;

            var startInfo = new ProcessStartInfo
            {
                FileName = "osascript",
                Arguments = $"-e \"{EscapeAppleScript(script)}\"",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = Encoding.UTF8
            };

            using var process = Process.Start(startInfo);
            if (process != null)
            {
                var output = process.StandardOutput.ReadToEnd().Trim();
                process.WaitForExit(60000);

                if (process.ExitCode == 0 && !string.IsNullOrWhiteSpace(output))
                {
                    var folderPath = output.StartsWith("alias ") ? output.Substring(6).Trim() : output;
                    if (folderPath.StartsWith("\"") && folderPath.EndsWith("\""))
                    {
                        folderPath = folderPath.Substring(1, folderPath.Length - 2);
                    }
                    _logger.LogInformation("User selected folder: {FolderPath}", folderPath);
                    return folderPath;
                }
                else
                {
                    _logger.LogInformation("User canceled folder dialog");
                    return null;
                }
            }

            _logger.LogWarning("Failed to start folder dialog process");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error showing open folder dialog");
            return null;
        }
    }

    public DialogResult ShowMessageBox(string message, string title, DialogButtons buttons)
    {
        try
        {
            _logger.LogDebug("Showing message box, title: {Title}, button type: {Buttons}", title, buttons);

            var escapedMessage = EscapeAppleScript(message ?? string.Empty);
            var escapedTitle = EscapeAppleScript(title ?? string.Empty);

            string buttonList;
            string defaultButton;
            switch (buttons)
            {
                case DialogButtons.OKCancel:
                    buttonList = "buttons {\"OK\", \"Cancel\"}";
                    defaultButton = "default button \"OK\"";
                    break;
                case DialogButtons.YesNo:
                    buttonList = "buttons {\"Yes\", \"No\"}";
                    defaultButton = "default button \"Yes\"";
                    break;
                case DialogButtons.YesNoCancel:
                    buttonList = "buttons {\"Yes\", \"No\", \"Cancel\"}";
                    defaultButton = "default button \"Yes\"";
                    break;
                default:
                    buttonList = "buttons {\"OK\"}";
                    defaultButton = "default button \"OK\"";
                    break;
            }

            var script = $"display dialog \"{escapedMessage}\" with title \"{escapedTitle}\" {buttonList} {defaultButton}";

            var startInfo = new ProcessStartInfo
            {
                FileName = "osascript",
                Arguments = $"-e \"{EscapeAppleScript(script)}\"",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = Encoding.UTF8
            };

            using var process = Process.Start(startInfo);
            if (process != null)
            {
                var output = process.StandardOutput.ReadToEnd().Trim();
                process.WaitForExit(60000);

                if (process.ExitCode != 0)
                {
                    _logger.LogInformation("Message box dismissed or canceled, exit code: {Code}", process.ExitCode);
                    return DialogResult.Cancel;
                }

                _logger.LogInformation("Message box output: {Output}", output);

                if (output.Contains("OK", StringComparison.OrdinalIgnoreCase))
                    return DialogResult.OK;
                if (output.Contains("Yes", StringComparison.OrdinalIgnoreCase))
                    return DialogResult.Yes;
                if (output.Contains("No", StringComparison.OrdinalIgnoreCase))
                    return DialogResult.No;
                if (output.Contains("Cancel", StringComparison.OrdinalIgnoreCase))
                    return DialogResult.Cancel;

                return DialogResult.None;
            }

            _logger.LogWarning("Failed to start message box process, using fallback");
            return buttons == DialogButtons.OK ? DialogResult.OK : DialogResult.Cancel;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error showing message box, using fallback");
            return buttons == DialogButtons.OK ? DialogResult.OK : DialogResult.Cancel;
        }
    }

    public bool ShowConfirmation(string message, string title)
    {
        try
        {
            _logger.LogDebug("Showing confirmation dialog, title: {Title}", title);
            var result = ShowMessageBox(message, title, DialogButtons.OKCancel);
            var confirmed = result == DialogResult.OK || result == DialogResult.Yes;
            _logger.LogInformation("User confirmation result: {Confirmed}", confirmed);
            return confirmed;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error showing confirmation dialog");
            return false;
        }
    }
}
