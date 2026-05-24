namespace PCL_CE.Neo.UI.Services;

public class WindowService : Core.Abstractions.IWindowService
{
    public event EventHandler? WindowClosed;
    public event EventHandler? WindowMinimized;
    public event EventHandler? WindowMaximized;
    public event EventHandler? WindowRestored;

    public object? MainWindow { get; private set; }
    private string _title = "PCL-CE.Neo";
    private int _width = 1200;
    private int _height = 800;
    private int _left = 100;
    private int _top = 100;
    private bool _isVisible = false;
    private bool _isMaximized = false;
    private bool _isTopmost = false;

    public void Initialize()
    {
#if WINDOWS
        InitializeWindowsWindow();
#elif MACCATALYST
        InitializeMacOSWindow();
#elif LINUX
        InitializeLinuxWindow();
#endif
    }

    public void ShowMainWindow()
    {
        _isVisible = true;
#if WINDOWS
        ShowWindowsWindow();
#elif MACCATALYST
        ShowMacOSWindow();
#elif LINUX
        ShowLinuxWindow();
#endif
    }

    public void CloseMainWindow()
    {
        _isVisible = false;
        WindowClosed?.Invoke(this, EventArgs.Empty);
#if WINDOWS
        CloseWindowsWindow();
#elif MACCATALYST
        CloseMacOSWindow();
#elif LINUX
        CloseLinuxWindow();
#endif
    }

    public void SetTitle(string title)
    {
        _title = title;
#if WINDOWS
        SetWindowsTitle(title);
#elif MACCATALYST
        SetMacOSTitle(title);
#elif LINUX
        SetLinuxTitle(title);
#endif
    }

    public void SetSize(int width, int height)
    {
        _width = width;
        _height = height;
#if WINDOWS
        SetWindowsSize(width, height);
#elif MACCATALYST
        SetMacOSSize(width, height);
#elif LINUX
        SetLinuxSize(width, height);
#endif
    }

    public void SetPosition(int x, int y)
    {
        _left = x;
        _top = y;
#if WINDOWS
        SetWindowsPosition(x, y);
#elif MACCATALYST
        SetMacOSPosition(x, y);
#elif LINUX
        SetLinuxPosition(x, y);
#endif
    }

    public void Minimize()
    {
        _isVisible = false;
        WindowMinimized?.Invoke(this, EventArgs.Empty);
#if WINDOWS
        MinimizeWindowsWindow();
#elif MACCATALYST
        MinimizeMacOSWindow();
#elif LINUX
        MinimizeLinuxWindow();
#endif
    }

    public void Maximize()
    {
        _isMaximized = true;
        WindowMaximized?.Invoke(this, EventArgs.Empty);
#if WINDOWS
        MaximizeWindowsWindow();
#elif MACCATALYST
        MaximizeMacOSWindow();
#elif LINUX
        MaximizeLinuxWindow();
#endif
    }

    public void Restore()
    {
        _isMaximized = false;
        WindowRestored?.Invoke(this, EventArgs.Empty);
#if WINDOWS
        RestoreWindowsWindow();
#elif MACCATALYST
        RestoreMacOSWindow();
#elif LINUX
        RestoreLinuxWindow();
#endif
    }

    public void SetTopmost(bool topmost)
    {
        _isTopmost = topmost;
#if WINDOWS
        SetWindowsTopmost(topmost);
#elif MACCATALYST
        SetMacOSTopmost(topmost);
#elif LINUX
        SetLinuxTopmost(topmost);
#endif
    }

    public double GetSystemDpi()
    {
#if WINDOWS
        return GetWindowsDpi();
#elif MACCATALYST
        return GetMacOSDpi();
#elif LINUX
        return GetLinuxDpi();
#else
        return 96.0;
#endif
    }

#if WINDOWS
    private void InitializeWindowsWindow()
    {
        MainWindow = new System.Windows.Window();
        if (MainWindow is System.Windows.Window window)
        {
            window.Title = _title;
            window.Width = _width;
            window.Height = _height;
            window.Left = _left;
            window.Top = _top;
        }
    }

    private void ShowWindowsWindow()
    {
        if (MainWindow is System.Windows.Window window)
        {
            window.Show();
            window.Activate();
        }
    }

    private void CloseWindowsWindow()
    {
        if (MainWindow is System.Windows.Window window)
        {
            window.Close();
        }
    }

    private void SetWindowsTitle(string title)
    {
        if (MainWindow is System.Windows.Window window)
        {
            window.Title = title;
        }
    }

    private void SetWindowsSize(int width, int height)
    {
        if (MainWindow is System.Windows.Window window)
        {
            window.Width = width;
            window.Height = height;
        }
    }

    private void SetWindowsPosition(int x, int y)
    {
        if (MainWindow is System.Windows.Window window)
        {
            window.Left = x;
            window.Top = y;
        }
    }

    private void MinimizeWindowsWindow()
    {
        if (MainWindow is System.Windows.Window window)
        {
            window.WindowState = System.Windows.WindowState.Minimized;
        }
    }

    private void MaximizeWindowsWindow()
    {
        if (MainWindow is System.Windows.Window window)
        {
            window.WindowState = System.Windows.WindowState.Maximized;
        }
    }

    private void RestoreWindowsWindow()
    {
        if (MainWindow is System.Windows.Window window)
        {
            window.WindowState = System.Windows.WindowState.Normal;
        }
    }

    private void SetWindowsTopmost(bool topmost)
    {
        if (MainWindow is System.Windows.Window window)
        {
            window.Topmost = topmost;
        }
    }

    private double GetWindowsDpi()
    {
        if (MainWindow is System.Windows.Window window)
        {
            var source = System.Windows.Interop.HwndSource.FromVisual(window);
            if (source != null)
            {
                return source.CompositionTarget.TransformToDevice.M11 * 96;
            }
        }
        return 96.0;
    }
#endif

#if MACCATALYST
    private void InitializeMacOSWindow()
    {
    }

    private void ShowMacOSWindow()
    {
        try
        {
            var script = $"tell application \"System Events\" to set frontmost of process \"PCL-CE.Neo\" to true";
            var process = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "osascript",
                    Arguments = $"-e '{script}'",
                    UseShellExecute = false
                }
            };
            process.Start();
        }
        catch
        {
        }
    }

    private void CloseMacOSWindow()
    {
    }

    private void SetMacOSTitle(string title)
    {
    }

    private void SetMacOSSize(int width, int height)
    {
    }

    private void SetMacOSPosition(int x, int y)
    {
    }

    private void MinimizeMacOSWindow()
    {
    }

    private void MaximizeMacOSWindow()
    {
    }

    private void RestoreMacOSWindow()
    {
    }

    private void SetMacOSTopmost(bool topmost)
    {
    }

    private double GetMacOSDpi()
    {
        try
        {
            var process = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "sysctl",
                    Arguments = "-n hw.displayscale",
                    UseShellExecute = false,
                    RedirectStandardOutput = true
                }
            };
            process.Start();
            var output = process.StandardOutput.ReadLine();
            process.WaitForExit(2000);

            if (!string.IsNullOrEmpty(output) && double.TryParse(output, out var scale))
            {
                return 96.0 * scale;
            }
        }
        catch
        {
        }
        return 144.0;
    }
#endif

#if LINUX
    private void InitializeLinuxWindow()
    {
    }

    private void ShowLinuxWindow()
    {
    }

    private void CloseLinuxWindow()
    {
    }

    private void SetLinuxTitle(string title)
    {
    }

    private void SetLinuxSize(int width, int height)
    {
    }

    private void SetLinuxPosition(int x, int y)
    {
    }

    private void MinimizeLinuxWindow()
    {
    }

    private void MaximizeLinuxWindow()
    {
    }

    private void RestoreLinuxWindow()
    {
    }

    private void SetLinuxTopmost(bool topmost)
    {
    }

    private double GetLinuxDpi()
    {
        try
        {
            var process = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "xrandr",
                    Arguments = "--current",
                    UseShellExecute = false,
                    RedirectStandardOutput = true
                }
            };
            process.Start();
            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit(3000);

            var match = System.Text.RegularExpressions.Regex.Match(output, @"(\d+)dpi");
            if (match.Success && int.TryParse(match.Groups[1].Value, out var dpi))
            {
                return dpi;
            }
        }
        catch
        {
        }
        return 96.0;
    }
#endif
}
