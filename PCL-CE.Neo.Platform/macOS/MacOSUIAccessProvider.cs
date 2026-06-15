using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Logging;
using PCL_CE.Neo.Core.Abstractions;

namespace PCL_CE.Neo.Platform.macOS;

public class MacOSUIAccessProvider : IUIAccessProvider
{
    private readonly ILogger<MacOSUIAccessProvider> _logger;
    private readonly SynchronizationContext _uiContext;
    private readonly int _uiThreadId;

    public MacOSUIAccessProvider() : this(Microsoft.Extensions.Logging.Abstractions.NullLogger<MacOSUIAccessProvider>.Instance)
    {
    }

    public MacOSUIAccessProvider(ILogger<MacOSUIAccessProvider> logger)
    {
        _logger = logger;
        try
        {
            _logger.LogDebug("Initializing macOS UI access provider");
            _uiContext = SynchronizationContext.Current ?? new SynchronizationContext();
            _uiThreadId = Thread.CurrentThread.ManagedThreadId;
            _logger.LogInformation("macOS UI access provider initialized, UI thread ID: {ThreadId}", _uiThreadId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing macOS UI access provider");
            _uiContext = new SynchronizationContext();
            _uiThreadId = Thread.CurrentThread.ManagedThreadId;
        }
    }

    public void Invoke(Action action)
    {
        try
        {
            if (action == null)
            {
                _logger.LogWarning("Attempted to invoke null action, ignored");
                return;
            }

            if (Thread.CurrentThread.ManagedThreadId == _uiThreadId)
            {
                _logger.LogDebug("Current thread is already UI thread, executing action directly");
                action();
                return;
            }

            _logger.LogDebug("Invoking UI action via SynchronizationContext");
            var completed = false;
            _uiContext.Send(_ =>
            {
                try
                {
                    action();
                    completed = true;
                }
                catch (Exception innerEx)
                {
                    _logger.LogError(innerEx, "Error executing UI action");
                }
            }, null);

            if (completed)
            {
                _logger.LogDebug("UI action completed");
            }
            else
            {
                _logger.LogWarning("UI action completed but status was not marked as completed");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invoking UI action");
        }
    }

    public async Task InvokeAsync(Action action)
    {
        try
        {
            if (action == null)
            {
                _logger.LogWarning("Attempted to invoke null async action, ignored");
                return;
            }

            if (Thread.CurrentThread.ManagedThreadId == _uiThreadId)
            {
                _logger.LogDebug("Current thread is already UI thread, executing async action directly");
                action();
                return;
            }

            _logger.LogDebug("Invoking UI async action via SynchronizationContext");
            var tcs = new TaskCompletionSource<bool>();
            _uiContext.Post(_ =>
            {
                try
                {
                    action();
                    tcs.TrySetResult(true);
                    _logger.LogDebug("Async UI action completed");
                }
                catch (Exception innerEx)
                {
                    tcs.TrySetException(innerEx);
                    _logger.LogError(innerEx, "Error executing async UI action");
                }
            }, null);

            await tcs.Task;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during async UI action invocation");
        }
    }

    public bool CheckAccess()
    {
        try
        {
            var access = Thread.CurrentThread.ManagedThreadId == _uiThreadId;
            _logger.LogDebug("Checking UI thread access: {Access}, current thread: {Current}, UI thread: {UI}",
                access, Thread.CurrentThread.ManagedThreadId, _uiThreadId);
            return access;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking UI thread access");
            return true;
        }
    }

    public double GetScreenDpi()
    {
        try
        {
            _logger.LogDebug("Getting screen DPI");
            double dpi = 72.0;
            var script = "tell application \"System Events\" to tell process \"Finder\" to get bounds of window of desktop";
            var startInfo = new ProcessStartInfo
            {
                FileName = "osascript",
                Arguments = $"-e \"{script}\"",
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
                _logger.LogDebug("Desktop bounds: {Output}", output);
            }

            _logger.LogInformation("Screen DPI: {Dpi}", dpi);
            return dpi;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting screen DPI, returning default value 72");
            return 72.0;
        }
    }

    public (int Width, int Height) GetScreenSize()
    {
        try
        {
            _logger.LogDebug("Getting screen size");
            int width = 1920;
            int height = 1080;

            var script = "tell application \"System Events\" to get bounds of window of desktop";
            var startInfo = new ProcessStartInfo
            {
                FileName = "osascript",
                Arguments = $"-e \"{script}\"",
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

                var parts = output.Split(',', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 4 &&
                    int.TryParse(parts[2]?.Trim(), out var parsedWidth) &&
                    int.TryParse(parts[3]?.Trim(), out var parsedHeight) &&
                    parsedWidth > 0 && parsedHeight > 0)
                {
                    width = parsedWidth;
                    height = parsedHeight;
                    _logger.LogDebug("Got screen size via osascript: {Width}x{Height}", width, height);
                }
                else
                {
                    _logger.LogWarning("Unable to parse screen size from output: {Output}, using default values", output);
                }
            }
            else
            {
                _logger.LogWarning("Failed to start osascript process to get screen size, using default values");
            }

            _logger.LogInformation("Screen size: {Width}x{Height}", width, height);
            return (width, height);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting screen size, returning default 1920x1080");
            return (1920, 1080);
        }
    }
}
