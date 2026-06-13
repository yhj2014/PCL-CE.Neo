using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Logging;
using PCL_CE.Neo.Core.Abstractions;

namespace PCL_CE.Neo.Platform.Windows;

public class WindowsUIAccessProvider : IUIAccessProvider
{
    private readonly ILogger<WindowsUIAccessProvider> _logger;
    private readonly SynchronizationContext _uiContext;
    private readonly int _uiThreadId;

    public WindowsUIAccessProvider(ILogger<WindowsUIAccessProvider> logger)
    {
        _logger = logger;
        try
        {
            _logger.LogDebug("正在初始化 Windows UI 访问提供程序");
            _uiContext = SynchronizationContext.Current ?? new SynchronizationContext();
            _uiThreadId = Thread.CurrentThread.ManagedThreadId;
            _logger.LogInformation("Windows UI 访问提供程序初始化完成，UI 线程 ID: {ThreadId}", _uiThreadId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "初始化 Windows UI 访问提供程序时发生错误");
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
                _logger.LogWarning("尝试执行空操作，已忽略");
                return;
            }

            if (Thread.CurrentThread.ManagedThreadId == _uiThreadId)
            {
                _logger.LogDebug("当前线程已是 UI 线程，直接执行操作");
                action();
                return;
            }

            _logger.LogDebug("通过 SynchronizationContext 同步调用 UI 操作");
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
                    _logger.LogError(innerEx, "执行 UI 操作时发生错误");
                }
            }, null);

            if (completed)
            {
                _logger.LogDebug("UI 操作执行完成");
            }
            else
            {
                _logger.LogWarning("UI 操作执行完成但结果为未完成状态");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "调用 UI 操作时发生错误");
        }
    }

    public async Task InvokeAsync(Action action)
    {
        try
        {
            if (action == null)
            {
                _logger.LogWarning("尝试执行空异步操作，已忽略");
                return;
            }

            if (Thread.CurrentThread.ManagedThreadId == _uiThreadId)
            {
                _logger.LogDebug("当前线程已是 UI 线程，直接执行异步操作");
                action();
                return;
            }

            _logger.LogDebug("通过 SynchronizationContext 异步调用 UI 操作");
            var tcs = new TaskCompletionSource<bool>();
            _uiContext.Post(_ =>
            {
                try
                {
                    action();
                    tcs.TrySetResult(true);
                    _logger.LogDebug("异步 UI 操作执行完成");
                }
                catch (Exception innerEx)
                {
                    tcs.TrySetException(innerEx);
                    _logger.LogError(innerEx, "执行异步 UI 操作时发生错误");
                }
            }, null);

            await tcs.Task;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "异步调用 UI 操作时发生错误");
        }
    }

    public bool CheckAccess()
    {
        try
        {
            var access = Thread.CurrentThread.ManagedThreadId == _uiThreadId;
            _logger.LogDebug("检查 UI 线程访问: {Access}, 当前线程: {Current}, UI 线程: {UI}",
                access, Thread.CurrentThread.ManagedThreadId, _uiThreadId);
            return access;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "检查 UI 线程访问时发生错误");
            return true;
        }
    }

    public double GetScreenDpi()
    {
        try
        {
            _logger.LogDebug("获取屏幕 DPI");
            double dpi = 96.0;
            var script = @"
Add-Type -AssemblyName System.Windows.Forms
$graphics = [System.Windows.Forms.Screen]::PrimaryScreen.Bounds
Write-Output ([System.Windows.Forms.Screen]::PrimaryScreen.Bounds.Width)
";
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
                if (double.TryParse(output?.Trim(), out var pixelWidth) && pixelWidth > 0)
                {
                    _logger.LogDebug("通过 PowerShell 获取屏幕像素宽度: {Width}", pixelWidth);
                }
            }

            _logger.LogInformation("屏幕 DPI: {Dpi}", dpi);
            return dpi;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取屏幕 DPI 时发生错误，返回默认值 96");
            return 96.0;
        }
    }

    public (int Width, int Height) GetScreenSize()
    {
        try
        {
            _logger.LogDebug("获取屏幕尺寸");
            int width = 1920;
            int height = 1080;

            var script = @"
Add-Type -AssemblyName System.Windows.Forms
$screen = [System.Windows.Forms.Screen]::PrimaryScreen
Write-Output ('{0}x{1}' -f $screen.Bounds.Width, $screen.Bounds.Height)
";
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
                var parts = output?.Trim().Split('x');
                if (parts != null && parts.Length == 2 &&
                    int.TryParse(parts[0], out var parsedWidth) &&
                    int.TryParse(parts[1], out var parsedHeight) &&
                    parsedWidth > 0 && parsedHeight > 0)
                {
                    width = parsedWidth;
                    height = parsedHeight;
                    _logger.LogDebug("通过 PowerShell 获取屏幕尺寸: {Width}x{Height}", width, height);
                }
                else
                {
                    _logger.LogWarning("无法解析 PowerShell 输出: {Output}，使用默认值", output);
                }
            }
            else
            {
                _logger.LogWarning("无法启动 PowerShell 进程获取屏幕尺寸，使用默认值");
            }

            _logger.LogInformation("屏幕尺寸: {Width}x{Height}", width, height);
            return (width, height);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取屏幕尺寸时发生错误，返回默认值 1920x1080");
            return (1920, 1080);
        }
    }
}
