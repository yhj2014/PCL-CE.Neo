namespace PCL.CE.Neo.Core.Abstractions;

public interface IClipboardService
{
    string? GetText();
    void SetText(string text);

    byte[]? GetImage();
    void SetImage(byte[] imageData);

    void Clear();
}
