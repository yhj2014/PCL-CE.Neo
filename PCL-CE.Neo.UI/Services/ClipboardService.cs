namespace PCL_CE.Neo.UI.Services;

public class ClipboardService : Core.Abstractions.IClipboardService
{
    public string? GetText()
    {
#if WINDOWS || MACCATALYST || LINUX
        return GetClipboardText();
#else
        return GetClipboardText();
#endif
    }

    private string? GetClipboardText()
    {
#if WINDOWS || MACCATALYST || LINUX
        try
        {
            var content = Windows.ApplicationModel.DataTransfer.Clipboard.GetContent();
            if (content == null) return null;
            return content.Contains(Windows.ApplicationModel.DataTransfer.StandardDataFormats.Text)
                ? content.GetTextAsync().GetAwaiter().GetResult()
                : null;
        }
        catch
        {
            return null;
        }
#else
        return null;
#endif
    }

    public void SetText(string text)
    {
#if WINDOWS || MACCATALYST || LINUX
        SetClipboardText(text);
#else
        SetClipboardText(text);
#endif
    }

    private void SetClipboardText(string text)
    {
#if WINDOWS || MACCATALYST || LINUX
        try
        {
            var dataPackage = new Windows.ApplicationModel.DataTransfer.DataPackage();
            dataPackage.SetText(text);
            Windows.ApplicationModel.DataTransfer.Clipboard.SetContent(dataPackage);
        }
        catch
        {
        }
#else
#endif
    }

    public byte[]? GetImage()
    {
#if WINDOWS || MACCATALYST || LINUX
        return GetClipboardImage();
#else
        return GetClipboardImage();
#endif
    }

    private byte[]? GetClipboardImage()
    {
#if WINDOWS || MACCATALYST || LINUX
        try
        {
            var content = Windows.ApplicationModel.DataTransfer.Clipboard.GetContent();
            if (content == null) return null;
            if (!content.Contains(Windows.ApplicationModel.DataTransfer.StandardDataFormats.Bitmap)) return null;
            var streamRef = content.GetBitmapAsync().GetAwaiter().GetResult();
            var stream = streamRef.OpenReadAsync().GetAwaiter().GetResult();
            using var memoryStream = new System.IO.MemoryStream();
            stream.AsStreamForRead().CopyTo(memoryStream);
            return memoryStream.ToArray();
        }
        catch
        {
            return null;
        }
#else
        return null;
#endif
    }

    public void SetImage(byte[] imageData)
    {
#if WINDOWS || MACCATALYST || LINUX
        SetClipboardImage(imageData);
#else
        SetClipboardImage(imageData);
#endif
    }

    private void SetClipboardImage(byte[] imageData)
    {
#if WINDOWS || MACCATALYST || LINUX
        try
        {
            using var memoryStream = new System.IO.MemoryStream(imageData);
            var randomAccessStream = memoryStream.AsRandomAccessStream();
            var dataPackage = new Windows.ApplicationModel.DataTransfer.DataPackage();
            dataPackage.SetBitmap(Windows.Storage.Streams.RandomAccessStreamReference.CreateFromStream(randomAccessStream));
            Windows.ApplicationModel.DataTransfer.Clipboard.SetContent(dataPackage);
        }
        catch
        {
        }
#else
#endif
    }

    public void Clear()
    {
#if WINDOWS || MACCATALYST || LINUX
        try
        {
            Windows.ApplicationModel.DataTransfer.Clipboard.Clear();
        }
        catch
        {
        }
#else
#endif
    }
}
