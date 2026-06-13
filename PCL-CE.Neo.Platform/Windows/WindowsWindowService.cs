using System.Diagnostics;
using Microsoft.Extensions.Logging;
using PCL_CE.Neo.Core.Abstractions;

namespace PCL_CE.Neo.Platform.Windows;

public class WindowsWindowService : IWindowService
{
    private readonly ILogger<WindowsWindowService> _logger;
    private object? _mainWindow;
    private string _title = "PCL-CE.Neo";
    private int _width = 1280;
    private int _height = 720;
    private int _x = 100;
    private int _y = 100;
    private bool _isVisible;
    private bool _isMaximized;
    private bool _isTopmost;

    public WindowsWindowService() : this(Microsoft.Extensions.Logging.Abstractions.NullLogger<WindowsWindowService>.Instance) { }

    public WindowsWindowService(ILogger<WindowsWindowService> logger)
    {
        _logger = logger;
        try
        {
            _logger.LogDebug("WindowsWindowService initializing");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error during WindowsWindowService initialization");
        }
    }

    public object? MainWindow => _mainWindow;

    public void Initialize()
    {
        try
        {
            _logger.LogDebug("Window service initialized");
            _mainWindow = new object();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize window service");
        }
    }

    public void ShowMainWindow()
    {
        try
        {
            _isVisible = true;
            _logger.LogDebug("Showing main window");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to show main window");
        }
    }

    public void CloseMainWindow()
    {
        try
        {
            _isVisible = false;
            _logger.LogDebug("Closing main window");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to close main window");
        }
    }

    public void SetTitle(string title)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                _logger.LogWarning("SetTitle called with empty title");
                return;
            }

            _title = title;
            _logger.LogDebug("Window title set to: {Title}", title);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set window title");
        }
    }

    public void SetSize(int width, int height)
    {
        try
        {
            if (width <= 0 || height <= 0)
            {
                _logger.LogWarning("Invalid window size: {Width}x{Height}", width, height);
                return;
            }

            _width = width;
            _height = height;
            _logger.LogDebug("Window size set to: {Width}x{Height}", width, height);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set window size");
        }
    }

    public void SetPosition(int x, int y)
    {
        try
        {
            _x = x;
            _y = y;
            _logger.LogDebug("Window position set to: {X}, {Y}", x, y);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set window position");
        }
    }

    public void Minimize()
    {
        try
        {
            _isVisible = false;
            _isMaximized = false;
            _logger.LogDebug("Window minimized");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to minimize window");
        }
    }

    public void Maximize()
    {
        try
        {
            _isMaximized = true;
            _isVisible = true;
            _logger.LogDebug("Window maximized");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to maximize window");
        }
    }

    public void Restore()
    {
        try
        {
            _isMaximized = false;
            _isVisible = true;
            _logger.LogDebug("Window restored");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to restore window");
        }
    }

    public void SetTopmost(bool topmost)
    {
        try
        {
            _isTopmost = topmost;
            _logger.LogDebug("Window topmost set to: {Topmost}", topmost);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set topmost");
        }
    }

    public double GetSystemDpi()
    {
        try
        {
            using var process = Process.Start(new ProcessStartInfo
            {
                FileName = "powershell",
                Arguments = "-Command \"Write-Host (Get-CimInstance -ClassName Win32_DesktopMonitor).PixelsPerXLogicalInch\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            });

            if (process != null)
            {
                var output = process.StandardOutput.ReadToEnd().Trim();
                process.WaitForExit(5000);

                if (double.TryParse(output, out var dpi) && dpi > 0)
                {
                    return dpi;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to detect system DPI");
        }

        return 96.0;
    }
}
