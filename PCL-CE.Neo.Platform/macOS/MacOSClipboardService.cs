using PCL_CE.Neo.Core.Abstractions;

namespace PCL_CE.Neo.Platform.macOS;

public class MacOSClipboardService : IClipboardService
{
    public string? GetText()
    {
        try
        {
            var process = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "pbpaste",
                    RedirectStandardOutput = true,
                    UseShellExecute = false
                }
            };
            process.Start();
            return process.StandardOutput.ReadToEnd();
        }
        catch
        {
            return null;
        }
    }

    public void SetText(string text)
    {
        try
        {
            var process = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "pbcopy",
                    RedirectStandardInput = true,
                    UseShellExecute = false
                }
            };
            process.Start();
            process.StandardInput.Write(text);
            process.StandardInput.Close();
            process.WaitForExit();
        }
        catch
        {
        }
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
        SetText(string.Empty);
    }
}
