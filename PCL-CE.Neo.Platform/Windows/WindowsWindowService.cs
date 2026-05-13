using System.Windows;
using PCL_CE.Neo.Core.Abstractions;

namespace PCL_CE.Neo.Platform.Windows;

public class WindowsWindowService : IWindowService
{
    private Window? _mainWindow;

    public object? MainWindow => _mainWindow;

    public void Initialize()
    {
    }

    public void ShowMainWindow()
    {
        if (_mainWindow != null)
        {
            _mainWindow.Show();
            _mainWindow.Activate();
        }
    }

    public void CloseMainWindow()
    {
        _mainWindow?.Close();
    }

    public void SetTitle(string title)
    {
        if (_mainWindow != null)
        {
            _mainWindow.Title = title;
        }
    }

    public void SetSize(int width, int height)
    {
        if (_mainWindow != null)
        {
            _mainWindow.Width = width;
            _mainWindow.Height = height;
        }
    }

    public void SetPosition(int x, int y)
    {
        if (_mainWindow != null)
        {
            _mainWindow.Left = x;
            _mainWindow.Top = y;
        }
    }

    public void Minimize()
    {
        if (_mainWindow != null)
        {
            _mainWindow.WindowState = WindowState.Minimized;
        }
    }

    public void Maximize()
    {
        if (_mainWindow != null)
        {
            _mainWindow.WindowState = WindowState.Maximized;
        }
    }

    public void Restore()
    {
        if (_mainWindow != null)
        {
            _mainWindow.WindowState = WindowState.Normal;
        }
    }

    public void SetTopmost(bool topmost)
    {
        if (_mainWindow != null)
        {
            _mainWindow.Topmost = topmost;
        }
    }

    public double GetSystemDpi()
    {
        if (_mainWindow != null)
        {
            var presentationSource = System.Windows.Interop.HwndSource.FromVisual(_mainWindow);
            if (presentationSource != null)
            {
                return presentationSource.CompositionTarget.TransformToDevice.M11 * 96;
            }
        }
        return 96.0;
    }

    internal void SetMainWindow(Window window)
    {
        _mainWindow = window;
    }
}
