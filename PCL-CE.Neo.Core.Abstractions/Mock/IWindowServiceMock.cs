namespace PCL_CE.Neo.Core.Abstractions.Mock;

public class WindowServiceMock : IWindowService
{
    public event EventHandler? WindowClosed;
    public event EventHandler? WindowMinimized;
    
    public double Width { get; set; } = 1200;
    public double Height { get; set; } = 800;
    public double Left { get; set; } = 100;
    public double Top { get; set; } = 100;
    public bool IsVisible { get; set; } = true;
    public string Title { get; set; } = "Test Window";
    
    public void SetSize(double width, double height)
    {
        Width = width;
        Height = height;
    }

    public (double Width, double Height) GetSize()
    {
        return (Width, Height);
    }

    public void SetPosition(double left, double top)
    {
        Left = left;
        Top = top;
    }

    public (double Left, double Top) GetPosition()
    {
        return (Left, Top);
    }

    public void SetTitle(string title)
    {
        Title = title;
    }

    public string GetTitle()
    {
        return Title;
    }

    public void Minimize()
    {
        IsVisible = false;
        WindowMinimized?.Invoke(this, EventArgs.Empty);
    }

    public void Show()
    {
        IsVisible = true;
    }

    public void Hide()
    {
        IsVisible = false;
    }

    public void Close()
    {
        IsVisible = false;
        WindowClosed?.Invoke(this, EventArgs.Empty);
    }

    public bool GetIsVisible()
    {
        return IsVisible;
    }
}
