namespace PCL_CE.Neo.Core.Abstractions.Mock;

public class ClipboardServiceMock : IClipboardService
{
    public string? CurrentText { get; private set; }
    public byte[]? CurrentImage { get; private set; }
    
    public void SetText(string text)
    {
        CurrentText = text;
    }

    public string GetText()
    {
        return CurrentText ?? "";
    }

    public void SetImage(byte[] imageData)
    {
        CurrentImage = imageData;
    }

    public byte[] GetImage()
    {
        return CurrentImage ?? Array.Empty<byte>();
    }

    public void Clear()
    {
        CurrentText = null;
        CurrentImage = null;
    }
}
