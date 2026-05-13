namespace PCL.CE.Neo.Core.Abstractions;

public interface IUIAccessProvider
{
    void Invoke(Action action);
    Task InvokeAsync(Action action);

    bool CheckAccess();

    double GetScreenDpi();
    (int Width, int Height) GetScreenSize();
}
