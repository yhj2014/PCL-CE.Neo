namespace PCL_CE.Neo.UI.Services;

public class UIAccessProvider : Core.Abstractions.IUIAccessProvider
{
    private double _cachedDpi = 96.0;
    private (int Width, int Height) _cachedScreenSize = (1920, 1080);
    private bool _initialized = false;

    public double GetScreenDpi()
    {
        if (!_initialized)
        {
            InitializeDisplayInfo();
        }
        return _cachedDpi;
    }

    public (int Width, int Height) GetScreenSize()
    {
        if (!_initialized)
        {
            InitializeDisplayInfo();
        }
        return _cachedScreenSize;
    }

    public void Invoke(Action action)
    {
#if WINDOWS || MACCATALYST || LINUX
        ExecuteOnUIThread(action);
#else
        action();
#endif
    }

    public Task InvokeAsync(Action action)
    {
#if WINDOWS || MACCATALYST || LINUX
        return ExecuteOnUIThreadAsync(action);
#else
        action();
        return Task.CompletedTask;
#endif
    }

    public bool CheckAccess()
    {
#if WINDOWS || MACCATALYST || LINUX
        return IsOnUIThread();
#else
        return true;
#endif
    }

    private void InitializeDisplayInfo()
    {
#if WINDOWS
        InitializeWindowsDisplayInfo();
#elif MACCATALYST
        InitializeMacOSDisplayInfo();
#elif LINUX
        InitializeLinuxDisplayInfo();
#endif
        _initialized = true;
    }

#if WINDOWS
    private void InitializeWindowsDisplayInfo()
    {
        try
        {
            var dpiX = 96.0;
            var dpiY = 96.0;

            using var graphics = System.Drawing.Graphics.FromHwnd(IntPtr.Zero);
            dpiX = graphics.DpiX;
            dpiY = graphics.DpiY;
            _cachedDpi = Math.Max(dpiX, dpiY);

            var screen = System.Windows.Forms.Screen.PrimaryScreen;
            if (screen != null)
            {
                _cachedScreenSize = (screen.Bounds.Width, screen.Bounds.Height);
            }
        }
        catch
        {
            _cachedDpi = 96.0;
            _cachedScreenSize = (1920, 1080);
        }
    }

    private void ExecuteOnUIThread(Action action)
    {
        try
        {
            var dispatcher = System.Windows.Application.Current?.Dispatcher;
            if (dispatcher != null && !dispatcher.CheckAccess())
            {
                dispatcher.Invoke(action);
            }
            else
            {
                action();
            }
        }
        catch
        {
            action();
        }
    }

    private Task ExecuteOnUIThreadAsync(Action action)
    {
        try
        {
            var dispatcher = System.Windows.Application.Current?.Dispatcher;
            if (dispatcher != null && !dispatcher.CheckAccess())
            {
                return dispatcher.InvokeAsync(action).Task;
            }
        }
        catch
        {
        }
        action();
        return Task.CompletedTask;
    }

    private bool IsOnUIThread()
    {
        try
        {
            var dispatcher = System.Windows.Application.Current?.Dispatcher;
            return dispatcher == null || dispatcher.CheckAccess();
        }
        catch
        {
            return true;
        }
    }
#endif

#if MACCATALYST
    private void InitializeMacOSDisplayInfo()
    {
        try
        {
            var script = @"
tell application ""Finder""
    set _bounds to bounds of window of desktop
    set _width to item 3 of _bounds
    set _height to item 4 of _bounds
    return _width & "","" & _height
end tell";

            var process = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "osascript",
                    Arguments = $"-e '{script}'",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }
            };
            process.Start();
            var output = process.StandardOutput.ReadLine();
            process.WaitForExit(3000);

            if (!string.IsNullOrEmpty(output))
            {
                var parts = output.Split(',');
                if (parts.Length >= 2 && int.TryParse(parts[0], out var width) && int.TryParse(parts[1], out var height))
                {
                    _cachedScreenSize = (width, height);
                }
            }

            var dpiScript = "system attribute \"sysctl\"";
            var dpiProcess = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "sysctl",
                    Arguments = "-n hw.displayscale",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }
            };
            dpiProcess.Start();
            var dpiOutput = dpiProcess.StandardOutput.ReadLine();
            dpiProcess.WaitForExit(2000);

            if (!string.IsNullOrEmpty(dpiOutput) && double.TryParse(dpiOutput, out var scale))
            {
                _cachedDpi = 96.0 * scale;
            }
            else
            {
                _cachedDpi = 144.0;
            }
        }
        catch
        {
            _cachedDpi = 144.0;
            _cachedScreenSize = (2560, 1600);
        }
    }

    private void ExecuteOnUIThread(Action action)
    {
        action();
    }

    private Task ExecuteOnUIThreadAsync(Action action)
    {
        action();
        return Task.CompletedTask;
    }

    private bool IsOnUIThread()
    {
        return true;
    }
#endif

#if LINUX
    private void InitializeLinuxDisplayInfo()
    {
        try
        {
            var xrandrProcess = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "xrandr",
                    Arguments = "--current",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }
            };
            xrandrProcess.Start();
            var xrandrOutput = xrandrProcess.StandardOutput.ReadToEnd();
            xrandrProcess.WaitForExit(3000);

            var lines = xrandrOutput.Split('\n');
            foreach (var line in lines)
            {
                if (line.Contains("current"))
                {
                    var match = System.Text.RegularExpressions.Regex.Match(line, @"(\d+)x(\d+)\s+\(\d+)x(\d+\.\d+\)");
                    if (match.Success)
                    {
                        if (int.TryParse(match.Groups[1].Value, out var width) && int.TryParse(match.Groups[2].Value, out var height))
                        {
                            _cachedScreenSize = (width, height);
                        }
                        if (int.TryParse(match.Groups[3].Value, out var dpi) && dpi > 0)
                        {
                            _cachedDpi = dpi;
                        }
                        break;
                    }
                }
            }

            if (_cachedDpi == 96.0)
            {
                var dpiProcess = new System.Diagnostics.Process
                {
                    StartInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "xdpyinfo",
                        Arguments = "",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    }
                };
                dpiProcess.Start();
                var dpiOutput = dpiProcess.StandardOutput.ReadToEnd();
                dpiProcess.WaitForExit(3000);

                var dpiMatch = System.Text.RegularExpressions.Regex.Match(dpiOutput, @"resolution:\s*(\d+)x(\d+)");
                if (dpiMatch.Success && int.TryParse(dpiMatch.Groups[1].Value, out var dpiX))
                {
                    _cachedDpi = dpiX;
                }
            }
        }
        catch
        {
            _cachedDpi = 96.0;
            _cachedScreenSize = (1920, 1080);
        }
    }

    private void ExecuteOnUIThread(Action action)
    {
        action();
    }

    private Task ExecuteOnUIThreadAsync(Action action)
    {
        action();
        return Task.CompletedTask;
    }

    private bool IsOnUIThread()
    {
        return true;
    }
#endif
}
