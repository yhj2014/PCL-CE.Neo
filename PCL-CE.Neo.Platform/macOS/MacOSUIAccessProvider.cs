namespace PCL_CE.Neo.Platform.macOS;

public class MacOSUIAccessProvider : Core.Abstractions.IUIAccessProvider
{
#if MACCATALYST
    public double ScreenDpi { get; set; } = 144;
    public (int Width, int Height) ScreenSize { get; set; } = (2560, 1600);
    public List<Action> PendingActions { get; private set; } = new List<Action>();

    public double GetScreenDpi()
    {
        return ScreenDpi;
    }

    public (int Width, int Height) GetScreenSize()
    {
        return ScreenSize;
    }

    public void Invoke(Action action)
    {
        PendingActions.Add(action);
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

    public void ExecuteAllPendingActions()
    {
        foreach (var action in PendingActions)
        {
            action();
        }
        PendingActions.Clear();
    }
#else
    public double GetScreenDpi()
    {
        throw new PlatformNotSupportedException("此功能在 macOS 上尚未实现");
    }

    public (int Width, int Height) GetScreenSize()
    {
        throw new PlatformNotSupportedException("此功能在 macOS 上尚未实现");
    }

    public void Invoke(Action action)
    {
        throw new PlatformNotSupportedException("此功能在 macOS 上尚未实现");
    }

    public Task InvokeAsync(Action action)
    {
        throw new PlatformNotSupportedException("此功能在 macOS 上尚未实现");
    }

    public bool CheckAccess()
    {
        throw new PlatformNotSupportedException("此功能在 macOS 上尚未实现");
    }
#endif
}
