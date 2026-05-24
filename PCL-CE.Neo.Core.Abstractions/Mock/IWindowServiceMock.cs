namespace PCL_CE.Neo.Core.Abstractions.Mock;

public class WindowServiceMock : IWindowService
{
    public event EventHandler? WindowClosed;
    public event EventHandler? WindowMinimized;
    public event EventHandler? WindowMaximized;
    public event EventHandler? WindowRestored;
    
    public object? MainWindow { get; private set; }
    public double Width { get; set; } = 1200;
    public double Height { get; set; } = 800;
    public double Left { get; set; } = 100;
    public double Top { get; set; } = 100;
    public bool IsVisible { get; set; } = true;
    public bool IsMaximized { get; set; } = false;
    public bool IsTopmost { get; set; } = false;
    public string Title { get; set; } = "Test Window";
    public double SystemDpi { get; set; } = 96;
    
    public void Initialize()
    {
        MainWindow = new object();
    }

    public void ShowMainWindow()
    {
        IsVisible = true;
    }

    public void CloseMainWindow()
    {
        IsVisible = false;
        WindowClosed?.Invoke(this, EventArgs.Empty);
    }

    public void SetSize(int width, int height)
    {
        Width = width;
        Height = height;
    }

    public void SetPosition(int x, int y)
    {
        Left = x;
        Top = y;
    }

    public void SetTitle(string title)
    {
        Title = title;
    }

    public void Minimize()
    {
        IsVisible = false;
        WindowMinimized?.Invoke(this, EventArgs.Empty);
    }

    public void Maximize()
    {
        IsMaximized = true;
        WindowMaximized?.Invoke(this, EventArgs.Empty);
    }

    public void Restore()
    {
        IsMaximized = false;
        WindowRestored?.Invoke(this, EventArgs.Empty);
    }

    public void SetTopmost(bool topmost)
    {
        IsTopmost = topmost;
    }

    public double GetSystemDpi()
    {
        return SystemDpi;
    }
}
