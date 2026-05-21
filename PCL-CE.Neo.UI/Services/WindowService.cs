namespace PCL_CE.Neo.UI.Services;

public class WindowService : Core.Abstractions.IWindowService
{
    public event EventHandler? WindowClosed;
    public event EventHandler? WindowMinimized;
    public event EventHandler? WindowMaximized;
    public event EventHandler? WindowRestored;

    public object? MainWindow { get; private set; }

    public void Initialize()
    {
#if WINDOWS || MACCATALYST || LINUX
        // TODO: Implement using Uno Platform window management
#endif
    }

    public void ShowMainWindow()
    {
#if WINDOWS || MACCATALYST || LINUX
        // TODO: Implement using Uno Platform window management
#endif
    }

    public void CloseMainWindow()
    {
#if WINDOWS || MACCATALYST || LINUX
        // TODO: Implement using Uno Platform window management
        WindowClosed?.Invoke(this, EventArgs.Empty);
#endif
    }

    public void SetTitle(string title)
    {
#if WINDOWS || MACCATALYST || LINUX
        // TODO: Implement using Uno Platform window management
#endif
    }

    public void SetSize(int width, int height)
    {
#if WINDOWS || MACCATALYST || LINUX
        // TODO: Implement using Uno Platform window management
#endif
    }

    public void SetPosition(int x, int y)
    {
#if WINDOWS || MACCATALYST || LINUX
        // TODO: Implement using Uno Platform window management
#endif
    }

    public void Minimize()
    {
#if WINDOWS || MACCATALYST || LINUX
        // TODO: Implement using Uno Platform window management
        WindowMinimized?.Invoke(this, EventArgs.Empty);
#endif
    }

    public void Maximize()
    {
#if WINDOWS || MACCATALYST || LINUX
        // TODO: Implement using Uno Platform window management
        WindowMaximized?.Invoke(this, EventArgs.Empty);
#endif
    }

    public void Restore()
    {
#if WINDOWS || MACCATALYST || LINUX
        // TODO: Implement using Uno Platform window management
        WindowRestored?.Invoke(this, EventArgs.Empty);
#endif
    }

    public void SetTopmost(bool topmost)
    {
#if WINDOWS || MACCATALYST || LINUX
        // TODO: Implement using Uno Platform window management
#endif
    }

    public double GetSystemDpi()
    {
#if WINDOWS || MACCATALYST || LINUX
        // TODO: Implement using Uno Platform display information
        return 96.0;
#else
        throw new PlatformNotSupportedException("WindowService requires Uno Platform");
#endif
    }
}
