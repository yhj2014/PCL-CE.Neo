namespace PCL_CE.Neo.Platform.macOS;

public class MacOSClipboardService : Core.Abstractions.IClipboardService
{
    private string? _text;
    private byte[]? _image;

    public string? GetText()
    {
        return _text;
    }

    public void SetText(string text)
    {
        _text = text;
    }

    public byte[]? GetImage()
    {
        return _image;
    }

    public void SetImage(byte[] imageData)
    {
        _image = imageData;
    }

    public void Clear()
    {
        _text = null;
        _image = null;
    }
}
