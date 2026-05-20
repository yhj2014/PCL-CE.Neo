using PCL_CE.Neo.Core.Abstractions;

namespace PCL_CE.Neo.Platform.Windows;

public class WindowsUIAccessProvider : IUIAccessProvider
{
    public void Invoke(Action action)
    {
        action();
    }

    public Task InvokeAsync(Action action)
    {
        action();
        return Task.CompletedTask;
    }

    public bool CheckAccess()
    {
        return true;
    }

    public double GetScreenDpi()
    {
        return 96.0;
    }

    public (int Width, int Height) GetScreenSize()
    {
        return (1920, 1080);
    }
}
