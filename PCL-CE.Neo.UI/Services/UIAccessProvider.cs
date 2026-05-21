namespace PCL_CE.Neo.UI.Services;

public class UIAccessProvider : Core.Abstractions.IUIAccessProvider
{
    public double GetScreenDpi()
    {
#if WINDOWS || MACCATALYST || LINUX
        // TODO: Implement using Uno Platform display information
        return 96.0;
#else
        throw new PlatformNotSupportedException("UIAccessProvider requires Uno Platform");
#endif
    }

    public (int Width, int Height) GetScreenSize()
    {
#if WINDOWS || MACCATALYST || LINUX
        // TODO: Implement using Uno Platform display information
        return (1920, 1080);
#else
        throw new PlatformNotSupportedException("UIAccessProvider requires Uno Platform");
#endif
    }

    public void Invoke(Action action)
    {
#if WINDOWS || MACCATALYST || LINUX
        action();
#else
        throw new PlatformNotSupportedException("UIAccessProvider requires Uno Platform");
#endif
    }

    public Task InvokeAsync(Action action)
    {
#if WINDOWS || MACCATALYST || LINUX
        action();
        return Task.CompletedTask;
#else
        throw new PlatformNotSupportedException("UIAccessProvider requires Uno Platform");
#endif
    }

    public bool CheckAccess()
    {
#if WINDOWS || MACCATALYST || LINUX
        return true;
#else
        throw new PlatformNotSupportedException("UIAccessProvider requires Uno Platform");
#endif
    }
}
