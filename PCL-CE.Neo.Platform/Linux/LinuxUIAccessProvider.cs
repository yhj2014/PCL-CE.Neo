using PCL_CE.Neo.Core.Abstractions;

namespace PCL_CE.Neo.Platform.Linux;

public class LinuxUIAccessProvider : IUIAccessProvider
{
    public double ScreenDpi { get; set; } = 96;
    public (int Width, int Height) ScreenSize { get; set; } = (1920, 1080);
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
}
