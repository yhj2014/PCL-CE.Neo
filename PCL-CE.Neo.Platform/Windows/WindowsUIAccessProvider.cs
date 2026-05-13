using System.Windows.Threading;

namespace PCL_CE.Neo.Platform.Windows;

public class WindowsUIAccessProvider : IUIAccessProvider
{
    private readonly Dispatcher? _dispatcher;

    public WindowsUIAccessProvider()
    {
        _dispatcher = System.Windows.Application.Current?.Dispatcher;
    }

    public void Invoke(Action action)
    {
        if (_dispatcher != null && !_dispatcher.CheckAccess())
        {
            _dispatcher.Invoke(action);
        }
        else
        {
            action();
        }
    }

    public async Task InvokeAsync(Action action)
    {
        if (_dispatcher != null && !_dispatcher.CheckAccess())
        {
            await _dispatcher.InvokeAsync(action);
        }
        else
        {
            action();
        }
    }

    public bool CheckAccess()
    {
        return _dispatcher == null || _dispatcher.CheckAccess();
    }

    public double GetScreenDpi()
    {
        var dpiX = 96.0;
        var dpiY = 96.0;

        try
        {
            using var graphics = System.Drawing.Graphics.FromHwnd(IntPtr.Zero);
            dpiX = graphics.DpiX;
            dpiY = graphics.DpiY;
        }
        catch
        {
            // Fallback to 96 DPI
        }

        return Math.Max(dpiX, dpiY);
    }

    public (int Width, int Height) GetScreenSize()
    {
        try
        {
            var screen = System.Windows.Forms.Screen.PrimaryScreen;
            if (screen != null)
            {
                return (screen.Bounds.Width, screen.Bounds.Height);
            }
        }
        catch
        {
            // Fallback to default
        }

        return (1920, 1080);
    }
}
