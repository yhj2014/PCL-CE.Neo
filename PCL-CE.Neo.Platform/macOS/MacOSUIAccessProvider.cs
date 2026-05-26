namespace PCL_CE.Neo.Platform.macOS;

public class MacOSUIAccessProvider : Core.Abstractions.IUIAccessProvider
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
