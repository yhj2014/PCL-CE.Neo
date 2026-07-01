namespace PCL_CE.Neo.Core.Utils.OS;

public static class ClipboardUtils
{
    public static void SetText(string text)
    {
        try
        {
            if (OperatingSystem.IsWindows())
                SetTextWindows(text);
            else if (OperatingSystem.IsMacOS())
                SetTextMacOS(text);
            else if (OperatingSystem.IsLinux())
                SetTextLinux(text);
        }
        catch
        {
        }
    }

    public static string? GetText()
    {
        try
        {
            if (OperatingSystem.IsWindows())
                return GetTextWindows();
            if (OperatingSystem.IsMacOS())
                return GetTextMacOS();
            if (OperatingSystem.IsLinux())
                return GetTextLinux();
        }
        catch
        {
        }
        return null;
    }

    private static void SetTextWindows(string text)
    {
        using var process = new System.Diagnostics.Process
        {
            StartInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c echo|set/p=\"{text.Replace("\"", "\\\"")}\" | clip",
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };
        process.Start();
        process.WaitForExit();
    }

    private static string? GetTextWindows()
    {
        using var process = new System.Diagnostics.Process
        {
            StartInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = "-Command \"Get-Clipboard -Raw\"",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true
            }
        };
        process.Start();
        string result = process.StandardOutput.ReadToEnd();
        process.WaitForExit();
        return result.TrimEnd();
    }

    private static void SetTextMacOS(string text)
    {
        using var process = new System.Diagnostics.Process
        {
            StartInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "pbcopy",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardInput = true
            }
        };
        process.Start();
        process.StandardInput.Write(text);
        process.StandardInput.Close();
        process.WaitForExit();
    }

    private static string? GetTextMacOS()
    {
        using var process = new System.Diagnostics.Process
        {
            StartInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "pbpaste",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true
            }
        };
        process.Start();
        string result = process.StandardOutput.ReadToEnd();
        process.WaitForExit();
        return result;
    }

    private static void SetTextLinux(string text)
    {
        using var process = new System.Diagnostics.Process
        {
            StartInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "xclip",
                Arguments = "-selection clipboard",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardInput = true
            }
        };
        process.Start();
        process.StandardInput.Write(text);
        process.StandardInput.Close();
        process.WaitForExit();
    }

    private static string? GetTextLinux()
    {
        using var process = new System.Diagnostics.Process
        {
            StartInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "xclip",
                Arguments = "-selection clipboard -o",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true
            }
        };
        process.Start();
        string result = process.StandardOutput.ReadToEnd();
        process.WaitForExit();
        return result;
    }
}