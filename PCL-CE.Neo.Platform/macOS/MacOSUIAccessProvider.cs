using PCL_CE.Neo.Core.Abstractions;

namespace PCL_CE.Neo.Platform.macOS;

public class MacOSUIAccessProvider : IUIAccessProvider
{
    public object? GetMainWindow()
    {
        return null;
    }

    public void SetMainWindow(object? window)
    {
    }

    public void InvokeOnUIThread(Action action)
    {
        action?.Invoke();
    }

    public T InvokeOnUIThread<T>(Func<T> func)
    {
        return func != null ? func() : default!;
    }
}
