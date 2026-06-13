using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Logging;
using PCL_CE.Neo.Core.Abstractions;

namespace PCL_CE.Neo.Platform.macOS;

public class MacOSClipboardService : IClipboardService
{
    private readonly ILogger<MacOSClipboardService> _logger;
    private string? _cachedText;
    private byte[]? _cachedImage;

    public MacOSClipboardService() : this(Microsoft.Extensions.Logging.Abstractions.NullLogger<MacOSClipboardService>.Instance) { }

    public MacOSClipboardService(ILogger<MacOSClipboardService> logger)
    {
        _logger = logger;
        try
        {
            _logger.LogDebug("Initializing macOS clipboard service");
            _cachedText = string.Empty;
            _cachedImage = null;
            _logger.LogInformation("macOS clipboard service initialized");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing macOS clipboard service");
        }
    }

    public string? GetText()
    {
        try
        {
            _logger.LogDebug("Getting text from clipboard");
            var startInfo = new ProcessStartInfo
            {
                FileName = "pbpaste",
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
                if (process.ExitCode == 0)
                {
                    var text = output;
                    _cachedText = text;
                    _logger.LogInformation("Successfully got text from clipboard, length: {Length}", text.Length);
                    return text;
                }
                else
                {
                    _logger.LogWarning("pbpaste failed, exit code: {ExitCode}", process.ExitCode);
                }
            }
            _logger.LogInformation("Using cached clipboard text");
            return _cachedText;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting text from clipboard");
            return _cachedText;
        }
    }

    public void SetText(string text)
    {
        try
        {
            if (text == null)
            {
                _logger.LogWarning("Attempted to set null text to clipboard, ignored");
                return;
            }

            _logger.LogDebug("Setting text to clipboard, length: {Length}", text.Length);
            _cachedText = text;
            var startInfo = new ProcessStartInfo
            {
                FileName = "pbcopy",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardInputEncoding = Encoding.UTF8
            };

            using var process = Process.Start(startInfo);
            if (process != null)
            {
                using (var writer = process.StandardInput)
                {
                    writer.Write(text);
                }
                process.WaitForExit(3000);
                _logger.LogInformation("Text successfully set to clipboard, length: {Length}", text.Length);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting text to clipboard");
        }
    }

    public byte[]? GetImage()
    {
        try
        {
            _logger.LogDebug("Getting image from clipboard");
            var tempPath = Path.Combine(Path.GetTempPath(), $"clipboard_image_{DateTime.Now.Ticks}.png");

            var script = $@"set tempPath to ""{tempPath.Replace("\"", "\\\"")}""
try
    set clipboardData to (the clipboard as JPEG picture)
    set fileRef to open for access file ((POSIX file tempPath) as string) with write permission
    write clipboardData to fileRef starting at 0
    close access fileRef
    return ""saved""
on error
    try
        set clipboardData to (the clipboard as TIFF picture)
        set fileRef to open for access file ((POSIX file tempPath) as string) with write permission
        write clipboardData to fileRef starting at 0
        close access fileRef
        return ""saved""
    on error
        return ""empty""
    end try
end try";

            var startInfo = new ProcessStartInfo
            {
                FileName = "osascript",
                Arguments = $"-e \"{script.Replace("\"", "\\\"")}\"",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using var process = Process.Start(startInfo);
            if (process != null)
            {
                var output = process.StandardOutput.ReadToEnd().Trim();
                process.WaitForExit(5000);

                if (output.Equals("saved", StringComparison.OrdinalIgnoreCase) && File.Exists(tempPath))
                {
                    var imageData = File.ReadAllBytes(tempPath);
                    try
                    {
                        File.Delete(tempPath);
                    }
                    catch { }
                    _cachedImage = imageData;
                    _logger.LogInformation("Successfully got image from clipboard, size: {Size} bytes", imageData.Length);
                    return imageData;
                }
                else
                {
                    _logger.LogWarning("Clipboard image read failed, output: {Output}", output);
                }
            }

            _logger.LogInformation("Using cached clipboard image (if available)");
            return _cachedImage;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting image from clipboard");
            return _cachedImage;
        }
    }

    public void SetImage(byte[] imageData)
    {
        try
        {
            if (imageData == null || imageData.Length == 0)
            {
                _logger.LogWarning("Attempted to set null or empty image data to clipboard, ignored");
                return;
            }

            _logger.LogDebug("Setting image to clipboard, size: {Size} bytes", imageData.Length);
            var tempPath = Path.Combine(Path.GetTempPath(), $"clipboard_set_image_{DateTime.Now.Ticks}.png");
            File.WriteAllBytes(tempPath, imageData);

            var script = $@"set imageFile to POSIX file ""{tempPath.Replace("\"", "\\\"")}""
try
    tell application ""System Events""
        set the clipboard to (read imageFile as JPEG picture)
    end tell
    return ""set""
on error
    try
        tell application ""System Events""
            set the clipboard to (read imageFile as TIFF picture)
        end tell
        return ""set""
    on error
        return ""failed""
    end try
end try";

            var startInfo = new ProcessStartInfo
            {
                FileName = "osascript",
                Arguments = $"-e \"{script.Replace("\"", "\\\"")}\"",
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
            catch { }

            _cachedImage = imageData;
            _logger.LogInformation("Image set to clipboard, size: {Size} bytes", imageData.Length);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting image to clipboard");
        }
    }

    public void Clear()
    {
        try
        {
            _logger.LogDebug("Clearing clipboard");
            _cachedText = null;
            _cachedImage = null;
            var startInfo = new ProcessStartInfo
            {
                FileName = "pbcopy",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using var process = Process.Start(startInfo);
            if (process != null)
            {
                process.StandardInput.Write(string.Empty);
                process.StandardInput.Close();
                process.WaitForExit(2000);
            }
            _logger.LogInformation("Clipboard cleared");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing clipboard");
        }
    }
}
