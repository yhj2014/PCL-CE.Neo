using System.Windows;
using PCL.Core.Abstractions;

namespace PCL.Platform.Windows;

public class WindowsClipboardService : IClipboardService
{
    public string? GetText()
    {
        if (Clipboard.ContainsText())
        {
            return Clipboard.GetText();
        }
        return null;
    }

    public void SetText(string text)
    {
        Clipboard.SetText(text);
    }

    public byte[]? GetImage()
    {
        if (Clipboard.ContainsImage())
        {
            var image = Clipboard.GetImage();
            if (image != null)
            {
                using var stream = new System.IO.MemoryStream();
                var encoder = new System.Windows.Media.Imaging.PngBitmapEncoder();
                encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(image));
                encoder.Save(stream);
                return stream.ToArray();
            }
        }
        return null;
    }

    public void SetImage(byte[] imageData)
    {
        using var stream = new System.IO.MemoryStream(imageData);
        var decoder = System.Windows.Media.Imaging.BitmapDecoder.Create(
            stream,
            System.Windows.Media.Imaging.BitmapCreateOptions.PreservePixelFormat,
            System.Windows.Media.Imaging.BitmapCacheOption.OnLoad
        );
        Clipboard.SetImage(decoder.Frames[0]);
    }

    public void Clear()
    {
        Clipboard.Clear();
    }
}
