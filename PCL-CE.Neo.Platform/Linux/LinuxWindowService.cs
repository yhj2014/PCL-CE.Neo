using PCL_CE.Neo.Core.Abstractions;

namespace PCL_CE.Neo.Platform.Linux;

public class LinuxWindowService : IWindowService
{
    public object? MainWindow => null;

    public void Initialize()
    {
    }

    public void ShowMainWindow()
    {
    }

    public void CloseMainWindow()
    {
    }

    public void SetTitle(string title)
    {
    }

    public void SetSize(int width, int height)
    {
    }

    public void SetPosition(int x, int y)
    {
    }

    public void Minimize()
    {
    }

    public void Maximize()
    {
    }

    public void Restore()
    {
    }

    public void SetTopmost(bool topmost)
    {
    }

    public double GetSystemDpi()
    {
        return 96.0;
    }
}
