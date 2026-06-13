using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Logging;
using PCL_CE.Neo.Core.Abstractions;

namespace PCL_CE.Neo.Platform.Windows;

public class WindowsClipboardService : IClipboardService
{
    private readonly ILogger<WindowsClipboardService> _logger;
    private string? _cachedText;
    private byte[]? _cachedImage;

    public WindowsClipboardService() : this(Microsoft.Extensions.Logging.Abstractions.NullLogger<WindowsClipboardService>.Instance) { }

    public WindowsClipboardService(ILogger<WindowsClipboardService> logger)
    {
        _logger = logger;
        try
        {
            _logger.LogDebug("正在初始化 Windows 剪贴板服务");
            _cachedText = string.Empty;
            _cachedImage = null;
            _logger.LogInformation("Windows 剪贴板服务初始化完成");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "初始化 Windows 剪贴板服务时发生错误");
        }
    }

    public string? GetText()
    {
        try
        {
            _logger.LogDebug("从剪贴板获取文本");
            var script = "Get-Clipboard -Format Text";
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
                process.WaitForExit(3000);
                if (process.ExitCode == 0 && !string.IsNullOrEmpty(output))
                {
                    var text = output.TrimEnd('\r', '\n');
                    _cachedText = text;
                    _logger.LogInformation("成功从剪贴板获取文本，长度: {Length}", text.Length);
                    return text;
                }
                else
                {
                    _logger.LogWarning("剪贴板文本读取失败或为空，退出码: {ExitCode}", process.ExitCode);
                }
            }
            _logger.LogInformation("使用缓存的剪贴板文本");
            return _cachedText;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "从剪贴板获取文本时发生错误");
            return _cachedText;
        }
    }

    public void SetText(string text)
    {
        try
        {
            if (text == null)
            {
                _logger.LogWarning("尝试设置空文本到剪贴板，已忽略");
                return;
            }

            _logger.LogDebug("设置文本到剪贴板，长度: {Length}", text.Length);
            var escapedText = text.Replace("\"", "`\"").Replace("\r", "`r").Replace("\n", "`n");
            var script = $"Set-Clipboard -Value \"{escapedText}\"";
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
            process?.WaitForExit(3000);
            _cachedText = text;
            _logger.LogInformation("文本已成功设置到剪贴板，长度: {Length}", text.Length);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "设置文本到剪贴板时发生错误");
        }
    }

    public byte[]? GetImage()
    {
        try
        {
            _logger.LogDebug("从剪贴板获取图像");
            var tempPath = Path.Combine(Path.GetTempPath(), $"clipboard_image_{DateTime.Now.Ticks}.png");
            var script = $"$img = Get-Clipboard -Format Image; if ($img -ne $null) {{ $img.Save('{tempPath.Replace("'", "''")}', [System.Drawing.Imaging.ImageFormat]::Png); Write-Output 'saved' }} else {{ Write-Output 'empty' }}";
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
            if (process != null)
            {
                var output = process.StandardOutput.ReadToEnd();
                process.WaitForExit(5000);

                if (output.Trim().Equals("saved", StringComparison.OrdinalIgnoreCase) && File.Exists(tempPath))
                {
                    var imageData = File.ReadAllBytes(tempPath);
                    try
                    {
                        File.Delete(tempPath);
                    }
                    catch
                    {
                    }
                    _cachedImage = imageData;
                    _logger.LogInformation("成功从剪贴板获取图像，大小: {Size} 字节", imageData.Length);
                    return imageData;
                }
                else
                {
                    _logger.LogWarning("剪贴板图像读取失败，输出: {Output}", output?.Trim());
                }
            }

            _logger.LogInformation("使用缓存的剪贴板图像（如可用）");
            return _cachedImage;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "从剪贴板获取图像时发生错误");
            return _cachedImage;
        }
    }

    public void SetImage(byte[] imageData)
    {
        try
        {
            if (imageData == null || imageData.Length == 0)
            {
                _logger.LogWarning("尝试设置空图像数据到剪贴板，已忽略");
                return;
            }

            _logger.LogDebug("设置图像到剪贴板，大小: {Size} 字节", imageData.Length);
            var tempPath = Path.Combine(Path.GetTempPath(), $"clipboard_set_image_{DateTime.Now.Ticks}.png");
            File.WriteAllBytes(tempPath, imageData);

            var escapedPath = tempPath.Replace("'", "''");
            var script = $"Add-Type -AssemblyName System.Windows.Forms; $img = [System.Drawing.Image]::FromFile('{escapedPath}'); [System.Windows.Forms.Clipboard]::SetImage($img); Write-Output 'set'";
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
            process?.WaitForExit(5000);
            try
            {
                File.Delete(tempPath);
            }
            catch
            {
            }

            _cachedImage = imageData;
            _logger.LogInformation("图像已设置到剪贴板，大小: {Size} 字节", imageData.Length);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "设置图像到剪贴板时发生错误");
        }
    }

    public void Clear()
    {
        try
        {
            _logger.LogDebug("清空剪贴板");
            var script = "Set-Clipboard -Value $null";
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
            process?.WaitForExit(2000);
            _cachedText = string.Empty;
            _cachedImage = null;
            _logger.LogInformation("剪贴板已清空");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "清空剪贴板时发生错误");
        }
    }
}
