using PCL.CE.Neo.Core.Abstractions;

namespace PCL.CE.Neo.Platform.Linux;

public class LinuxClipboardService : IClipboardService
{
    public string? GetText()
    {
        try
        {
            var process = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "xclip",
                    Arguments = "-selection clipboard -o",
                    RedirectStandardOutput = true,
                    UseShellExecute = false
                }
            };
            process.Start();
            var result = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            return result;
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
                    FileName = "xclip",
                    Arguments = "-selection clipboard",
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
