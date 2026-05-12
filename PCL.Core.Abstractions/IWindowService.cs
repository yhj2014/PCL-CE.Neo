namespace PCL.Core.Abstractions;

public interface IWindowService
{
    object? MainWindow { get; }

    void Initialize();
    void ShowMainWindow();
    void CloseMainWindow();

    void SetTitle(string title);
    void SetSize(int width, int height);
    void SetPosition(int x, int y);

    void Minimize();
    void Maximize();
    void Restore();

    void SetTopmost(bool topmost);

    double GetSystemDpi();
}
