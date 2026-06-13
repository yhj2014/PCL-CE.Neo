using System.Diagnostics;
using Microsoft.Extensions.Logging;
using PCL_CE.Neo.Core.Abstractions;

namespace PCL_CE.Neo.Platform.Linux;

public class LinuxUIAccessProvider : IUIAccessProvider
{
    private readonly ILogger<LinuxUIAccessProvider> _logger;
    private readonly SynchronizationContext? _mainContext;
    private readonly int _mainThreadId;

    public LinuxUIAccessProvider(ILogger<LinuxUIAccessProvider> logger)
    {
        _logger = logger;
        try
        {
            _logger.LogDebug("LinuxUIAccessProvider initializing");
            _mainContext = SynchronizationContext.Current;
            _mainThreadId = Environment.CurrentManagedThreadId;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error during LinuxUIAccessProvider initialization");
        }
    }

    public void Invoke(Action action)
    {
        try
        {
            if (action == null)
            {
                _logger.LogWarning("Invoke called with null action");
                return;
            }

            if (_mainContext != null && !CheckAccess())
            {
                _logger.LogDebug("Dispatching action to UI thread via SynchronizationContext");
                _mainContext.Post(_ =>
                {
                    try { action(); }
                    catch (Exception ex) { _logger.LogError(ex, "Action execution failed"); }
                }, null);
            }
            else
            {
                _logger.LogDebug("Executing action on current thread");
                action();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to invoke action");
        }
    }

    public async Task InvokeAsync(Action action)
    {
        try
        {
            if (action == null)
            {
                _logger.LogWarning("InvokeAsync called with null action");
                return;
            }

            if (_mainContext != null && !CheckAccess())
            {
                _logger.LogDebug("Dispatching async action to UI thread");
                var tcs = new TaskCompletionSource<bool>();
                _mainContext.Post(_ =>
                {
                    try { action(); tcs.TrySetResult(true); }
                    catch (Exception ex) { tcs.TrySetException(ex); }
                }, null);
                await tcs.Task;
            }
            else
            {
                _logger.LogDebug("Executing async action on current thread");
                action();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to invoke async action");
            throw;
        }
    }

    public bool CheckAccess()
    {
        try
        {
            return Environment.CurrentManagedThreadId == _mainThreadId;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "CheckAccess failed, assuming true");
            return true;
        }
    }

    public double GetScreenDpi()
    {
        try
        {
            using var process = Process.Start(new ProcessStartInfo
            {
                FileName = "xrandr",
                Arguments = "--current",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            });

            if (process != null)
            {
                var output = process.StandardOutput.ReadToEnd();
                process.WaitForExit(5000);

                var match = System.Text.RegularExpressions.Regex.Match(output, @"(\d+(?:\.\d+)?)\s*dpi");
                if (match.Success && double.TryParse(match.Groups[1].Value, out var dpi))
                {
                    return dpi;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get screen DPI via xrandr");
        }

        return 96.0;
    }

    public (int Width, int Height) GetScreenSize()
    {
        try
        {
            using var process = Process.Start(new ProcessStartInfo
            {
                FileName = "xrandr",
                Arguments = "--current",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            });

            if (process != null)
            {
                var output = process.StandardOutput.ReadToEnd();
                process.WaitForExit(5000);

                var match = System.Text.RegularExpressions.Regex.Match(output, @"current\s+(\d+)\s+x\s+(\d+)");
                if (match.Success
                    && int.TryParse(match.Groups[1].Value, out var width)
                    && int.TryParse(match.Groups[2].Value, out var height))
                {
                    return (width, height);
                }

                var primary = System.Text.RegularExpressions.Regex.Match(output, @"(\d+)x(\d+).*?\*");
                if (primary.Success
                    && int.TryParse(primary.Groups[1].Value, out var w)
                    && int.TryParse(primary.Groups[2].Value, out var h))
                {
                    return (w, h);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get screen size via xrandr");
        }

        return (1920, 1080);
    }
}
