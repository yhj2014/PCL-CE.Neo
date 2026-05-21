namespace PCL_CE.Neo.UI.Services;

public class ClipboardService : Core.Abstractions.IClipboardService
{
    public string? GetText()
    {
#if WINDOWS || MACCATALYST || LINUX
        // TODO: Implement using Uno Platform clipboard
        return null;
#else
        throw new PlatformNotSupportedException("ClipboardService requires Uno Platform");
#endif
    }

    public void SetText(string text)
    {
#if WINDOWS || MACCATALYST || LINUX
        // TODO: Implement using Uno Platform clipboard
#else
        throw new PlatformNotSupportedException("ClipboardService requires Uno Platform");
#endif
    }

    public byte[]? GetImage()
    {
#if WINDOWS || MACCATALYST || LINUX
        // TODO: Implement using Uno Platform clipboard
        return null;
#else
        throw new PlatformNotSupportedException("ClipboardService requires Uno Platform");
#endif
    }

    public void SetImage(byte[] imageData)
    {
#if WINDOWS || MACCATALYST || LINUX
        // TODO: Implement using Uno Platform clipboard
#else
        throw new PlatformNotSupportedException("ClipboardService requires Uno Platform");
#endif
    }

    public void Clear()
    {
#if WINDOWS || MACCATALYST || LINUX
        // TODO: Implement using Uno Platform clipboard
#else
        throw new PlatformNotSupportedException("ClipboardService requires Uno Platform");
#endif
    }
}
