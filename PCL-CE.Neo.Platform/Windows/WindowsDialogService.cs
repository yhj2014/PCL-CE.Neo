using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Logging;
using PCL_CE.Neo.Core.Abstractions;

namespace PCL_CE.Neo.Platform.Windows;

public class WindowsDialogService : IDialogService
{
    private readonly ILogger<WindowsDialogService> _logger;

    public WindowsDialogService(ILogger<WindowsDialogService> logger)
    {
        _logger = logger;
        try
        {
            _logger.LogDebug("正在初始化 Windows 对话框服务");
            _logger.LogInformation("Windows 对话框服务初始化完成");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "初始化 Windows 对话框服务时发生错误");
        }
    }

    public string? ShowOpenFileDialog(string filter, string? initialDirectory = null)
    {
        try
        {
            _logger.LogDebug("显示打开文件对话框，过滤器: {Filter}, 初始目录: {Dir}", filter, initialDirectory ?? "(默认)");
            var escapedFilter = string.IsNullOrEmpty(filter) ? "All Files (*.*)|*.*" : filter;
            var escapedDir = string.IsNullOrEmpty(initialDirectory) ? string.Empty : initialDirectory.Replace("'", "''");

            var script = $@"
Add-Type -AssemblyName System.Windows.Forms
$dlg = New-Object System.Windows.Forms.OpenFileDialog
$dlg.Filter = '{escapedFilter.Replace("'", "''")}'
$dlg.InitialDirectory = '{escapedDir}'
$result = $dlg.ShowDialog()
if ($result -eq [System.Windows.Forms.DialogResult]::OK) {{ Write-Output $dlg.FileName }} else {{ Write-Output '' }}
";

            var startInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-Command \"{script}\"",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = Encoding.UTF8
            };

            using var process = Process.Start(startInfo);
            if (process != null)
            {
                var output = process.StandardOutput.ReadToEnd();
                process.WaitForExit(30000);
                var filePath = output.Trim().Trim('\"');
                if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
                {
                    _logger.LogInformation("用户选择文件: {FilePath}", filePath);
                    return filePath;
                }
                else
                {
                    _logger.LogInformation("用户取消了文件对话框或选择的文件无效");
                    return null;
                }
            }

            _logger.LogWarning("无法启动文件对话框进程");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "显示打开文件对话框时发生错误");
            return null;
        }
    }

    public string? ShowSaveFileDialog(string filter, string defaultFileName, string? initialDirectory = null)
    {
        try
        {
            _logger.LogDebug("显示保存文件对话框，过滤器: {Filter}, 默认文件名: {Name}, 初始目录: {Dir}", filter, defaultFileName, initialDirectory ?? "(默认)");
            var escapedFilter = string.IsNullOrEmpty(filter) ? "All Files (*.*)|*.*" : filter;
            var escapedDir = string.IsNullOrEmpty(initialDirectory) ? string.Empty : initialDirectory.Replace("'", "''");
            var escapedName = (defaultFileName ?? string.Empty).Replace("'", "''");

            var script = $@"
Add-Type -AssemblyName System.Windows.Forms
$dlg = New-Object System.Windows.Forms.SaveFileDialog
$dlg.Filter = '{escapedFilter.Replace("'", "''")}'
$dlg.FileName = '{escapedName}'
$dlg.InitialDirectory = '{escapedDir}'
$result = $dlg.ShowDialog()
if ($result -eq [System.Windows.Forms.DialogResult]::OK) {{ Write-Output $dlg.FileName }} else {{ Write-Output '' }}
";

            var startInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-Command \"{script}\"",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = Encoding.UTF8
            };

            using var process = Process.Start(startInfo);
            if (process != null)
            {
                var output = process.StandardOutput.ReadToEnd();
                process.WaitForExit(30000);
                var filePath = output.Trim().Trim('\"');
                if (!string.IsNullOrEmpty(filePath))
                {
                    _logger.LogInformation("用户保存文件: {FilePath}", filePath);
                    return filePath;
                }
                else
                {
                    _logger.LogInformation("用户取消了保存文件对话框");
                    return null;
                }
            }

            _logger.LogWarning("无法启动保存文件对话框进程");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "显示保存文件对话框时发生错误");
            return null;
        }
    }

    public string? ShowOpenFolderDialog(string? initialDirectory = null)
    {
        try
        {
            _logger.LogDebug("显示打开文件夹对话框，初始目录: {Dir}", initialDirectory ?? "(默认)");
            var escapedDir = string.IsNullOrEmpty(initialDirectory) ? string.Empty : initialDirectory.Replace("'", "''");

            var script = $@"
Add-Type -AssemblyName System.Windows.Forms
$dlg = New-Object System.Windows.Forms.FolderBrowserDialog
$dlg.SelectedPath = '{escapedDir}'
$result = $dlg.ShowDialog()
if ($result -eq [System.Windows.Forms.DialogResult]::OK) {{ Write-Output $dlg.SelectedPath }} else {{ Write-Output '' }}
";

            var startInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-Command \"{script}\"",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = Encoding.UTF8
            };

            using var process = Process.Start(startInfo);
            if (process != null)
            {
                var output = process.StandardOutput.ReadToEnd();
                process.WaitForExit(30000);
                var folderPath = output.Trim().Trim('\"');
                if (!string.IsNullOrEmpty(folderPath) && Directory.Exists(folderPath))
                {
                    _logger.LogInformation("用户选择文件夹: {FolderPath}", folderPath);
                    return folderPath;
                }
                else
                {
                    _logger.LogInformation("用户取消了文件夹对话框或选择的文件夹无效");
                    return null;
                }
            }

            _logger.LogWarning("无法启动文件夹对话框进程");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "显示打开文件夹对话框时发生错误");
            return null;
        }
    }

    public DialogResult ShowMessageBox(string message, string title, DialogButtons buttons)
    {
        try
        {
            _logger.LogDebug("显示消息框，标题: {Title}, 按钮类型: {Buttons}", title, buttons);
            var escapedMessage = (message ?? string.Empty).Replace("'", "''").Replace("\"", "'\"'\"'\"");
            var escapedTitle = (title ?? string.Empty).Replace("'", "''").Replace("\"", "'\"'\"'\"");
            var buttonMapping = buttons switch
            {
                DialogButtons.OK => "[System.Windows.Forms.MessageBoxButtons]::OK",
                DialogButtons.OKCancel => "[System.Windows.Forms.MessageBoxButtons]::OKCancel",
                DialogButtons.YesNo => "[System.Windows.Forms.MessageBoxButtons]::YesNo",
                DialogButtons.YesNoCancel => "[System.Windows.Forms.MessageBoxButtons]::YesNoCancel",
                _ => "[System.Windows.Forms.MessageBoxButtons]::OK"
            };

            var script = $@"
Add-Type -AssemblyName System.Windows.Forms
$result = [System.Windows.Forms.MessageBox]::Show('{escapedMessage}', '{escapedTitle}', {buttonMapping})
Write-Output $result
";

            var startInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-Command \"{script}\"",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = Encoding.UTF8
            };

            using var process = Process.Start(startInfo);
            if (process != null)
            {
                var output = process.StandardOutput.ReadToEnd();
                process.WaitForExit(30000);
                var resultStr = output.Trim();
                _logger.LogInformation("用户在消息框中选择: {Result}", resultStr);
                var result = resultStr.ToUpperInvariant() switch
                {
                    "OK" => DialogResult.OK,
                    "CANCEL" => DialogResult.Cancel,
                    "YES" => DialogResult.Yes,
                    "NO" => DialogResult.No,
                    _ => DialogResult.None
                };
                return result;
            }

            _logger.LogWarning("无法启动消息框进程");
            return DialogResult.None;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "显示消息框时发生错误");
            return DialogResult.None;
        }
    }

    public bool ShowConfirmation(string message, string title)
    {
        try
        {
            _logger.LogDebug("显示确认对话框，标题: {Title}", title);
            var result = ShowMessageBox(message, title, DialogButtons.OKCancel);
            var confirmed = result == DialogResult.OK || result == DialogResult.Yes;
            _logger.LogInformation("用户确认结果: {Confirmed}", confirmed);
            return confirmed;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "显示确认对话框时发生错误");
            return false;
        }
    }
}
