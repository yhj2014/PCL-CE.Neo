using System.Diagnostics;
using Microsoft.Extensions.Logging;
using PCL_CE.Neo.Core.Abstractions;

namespace PCL_CE.Neo.Platform.Linux;

public class LinuxClipboardService : IClipboardService
{
    private readonly ILogger<LinuxClipboardService> _logger;

    public LinuxClipboardService() : this(Microsoft.Extensions.Logging.Abstractions.NullLogger<LinuxClipboardService>.Instance)
    {
    }

    public LinuxClipboardService(ILogger<LinuxClipboardService> logger)
    {
        _logger = logger;
        try
        {
            _logger.LogDebug("LinuxClipboardService initializing");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error during LinuxClipboardService initialization");
        }
    }

    private string? FindClipboardTool()
    {
        var tools = new[] { "xclip", "xsel" };
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

    public string? GetText()
    {
        try
        {
            var tool = FindClipboardTool();
            if (string.IsNullOrEmpty(tool))
            {
                _logger.LogWarning("No clipboard tool available (xclip/xsel not found)");
                return null;
            }

            var args = tool == "xclip"
                ? "-selection clipboard -o"
                : "--clipboard --output";

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
                var text = process.StandardOutput.ReadToEnd();
                process.WaitForExit(5000);

                _logger.LogDebug("Clipboard read: {Length} chars", text.Length);
                return text;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to read clipboard text");
        }

        return null;
    }

    public void SetText(string text)
    {
        try
        {
            if (text == null)
            {
                _logger.LogWarning("SetText called with null text");
                return;
            }

            var tool = FindClipboardTool();
            if (string.IsNullOrEmpty(tool))
            {
                _logger.LogWarning("No clipboard tool available, ignoring SetText");
                return;
            }

            var args = tool == "xclip"
                ? "-selection clipboard -i"
                : "--clipboard --input";

            var startInfo = new ProcessStartInfo
            {
                FileName = tool,
                Arguments = args,
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            if (process != null)
            {
                using var writer = process.StandardInput;
                writer.Write(text);
            }

            process?.WaitForExit(5000);
            _logger.LogDebug("Clipboard write: {Length} chars", text.Length);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to write clipboard text");
        }
    }

    public byte[]? GetImage()
    {
        try
        {
            var tool = FindClipboardTool();
            if (string.IsNullOrEmpty(tool))
            {
                _logger.LogWarning("No clipboard tool available");
                return null;
            }

            if (tool == "xclip")
            {
                using var process = Process.Start(new ProcessStartInfo
                {
                    FileName = "xclip",
                    Arguments = "-selection clipboard -t image/png -o",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                });

                if (process != null)
                {
                    using var ms = new MemoryStream();
                    process.StandardOutput.BaseStream.CopyTo(ms);
                    process.WaitForExit(5000);

                    var result = ms.ToArray();
                    if (result.Length > 0)
                    {
                        _logger.LogDebug("Clipboard image read: {Length} bytes", result.Length);
                        return result;
                    }
                }
            }

            _logger.LogDebug("No image in clipboard");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to read clipboard image");
            return null;
        }
    }

    public void SetImage(byte[] imageData)
    {
        try
        {
            if (imageData == null || imageData.Length == 0)
            {
                _logger.LogWarning("SetImage called with empty data");
                return;
            }

            var tool = FindClipboardTool();
            if (tool != "xclip")
            {
                _logger.LogWarning("Clipboard image write requires xclip, but it was not found");
                return;
            }

            using var process = Process.Start(new ProcessStartInfo
            {
                FileName = "xclip",
                Arguments = "-selection clipboard -t image/png -i",
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            });

            if (process != null)
            {
                using var writer = process.StandardInput.BaseStream;
                writer.Write(imageData, 0, imageData.Length);
            }

            process?.WaitForExit(5000);
            _logger.LogDebug("Clipboard image write: {Length} bytes", imageData.Length);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to write clipboard image");
        }
    }

    public void Clear()
    {
        try
        {
            var tool = FindClipboardTool();
            if (string.IsNullOrEmpty(tool))
            {
                _logger.LogDebug("No clipboard tool to clear");
                return;
            }

            var args = tool == "xclip"
                ? "-selection clipboard -i /dev/null"
                : "--clipboard --clear";

            using var process = Process.Start(new ProcessStartInfo
            {
                FileName = tool,
                Arguments = args,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            });

            process?.WaitForExit(3000);
            _logger.LogDebug("Clipboard cleared");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to clear clipboard");
        }
    }
}
