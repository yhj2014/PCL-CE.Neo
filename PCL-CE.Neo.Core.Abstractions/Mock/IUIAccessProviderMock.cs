namespace PCL_CE.Neo.Core.Abstractions.Mock;

public class UIAccessProviderMock : IUIAccessProvider
{
    public double ScreenDpi { get; set; } = 96;
    public (double Width, double Height) ScreenSize { get; set; } = (1920, 1080);
    public List<Action> PendingActions { get; private set; } = new List<Action>();
    
    public double GetScreenDpi()
    {
        return ScreenDpi;
    }

    public (double Width, double Height) GetScreenSize()
    {
        return ScreenSize;
    }

    public void Invoke(Action action)
    {
        PendingActions.Add(action);
        action();
    }

    public async Task InvokeAsync(Func<Task> action)
    {
        await action();
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
