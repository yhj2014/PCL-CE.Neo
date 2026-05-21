namespace PCL_CE.Neo.Platform.macOS;

public class MacOSClipboardService : Core.Abstractions.IClipboardService
{
#if MACCATALYST
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
#else
    public string? GetText()
    {
        throw new PlatformNotSupportedException("此功能在 macOS 上尚未实现");
    }

    public void SetText(string text)
    {
        throw new PlatformNotSupportedException("此功能在 macOS 上尚未实现");
    }

    public byte[]? GetImage()
    {
        throw new PlatformNotSupportedException("此功能在 macOS 上尚未实现");
    }

    public void SetImage(byte[] imageData)
    {
        throw new PlatformNotSupportedException("此功能在 macOS 上尚未实现");
    }

    public void Clear()
    {
        throw new PlatformNotSupportedException("此功能在 macOS 上尚未实现");
    }
#endif
}
