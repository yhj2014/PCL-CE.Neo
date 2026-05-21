using PCL_CE.Neo.Core.Abstractions;

namespace PCL_CE.Neo.Platform.Windows;

public class WindowsClipboardService : IClipboardService
{
    public string? GetText()
    {
        return null;
    }

    public void SetText(string text)
    {
    }

    public byte[]? GetImage()
    {
        return null;
    }

    public void SetImage(byte[] imageData)
    {
    }

    public void Clear()
    {
    }
}
