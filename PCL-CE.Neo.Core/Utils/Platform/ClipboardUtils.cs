using System.Runtime.InteropServices;

namespace PCL_CE.Neo.Core.Utils.Platform;

public static class ClipboardUtils
{
    public static bool SetText(string text)
    {
        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return SetTextWindows(text);
            }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return SetTextLinux(text);
            }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return SetTextMacOS(text);
            }
            return false;
        }
        catch
        {
            return false;
        }
    }

    public static string? GetText()
    {
        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return GetTextWindows();
            }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return GetTextLinux();
            }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return GetTextMacOS();
            }
            return null;
        }
        catch
        {
            return null;
        }
    }

    public static bool ContainsText()
    {
        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return ContainsTextWindows();
            }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return ContainsTextLinux();
            }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return ContainsTextMacOS();
            }
            return false;
        }
        catch
        {
            return false;
        }
    }

    private static bool SetTextWindows(string text)
    {
        if (!OpenClipboard(IntPtr.Zero))
            return false;
        try
        {
            EmptyClipboard();
            var hGlobal = Marshal.StringToHGlobalUni(text);
            try
            {
                SetClipboardData(13, hGlobal);
                return true;
            }
            finally
            {
            }
        }
        finally
        {
            CloseClipboard();
        }
    }

    private static string? GetTextWindows()
    {
        if (!OpenClipboard(IntPtr.Zero))
            return null;
        try
        {
            var hGlobal = GetClipboardData(13);
            if (hGlobal == IntPtr.Zero)
                return null;
            return Marshal.PtrToStringUni(hGlobal);
        }
        finally
        {
            CloseClipboard();
        }
    }

    private static bool ContainsTextWindows()
    {
        if (!OpenClipboard(IntPtr.Zero))
            return false;
        try
        {
            return IsClipboardFormatAvailable(13);
        }
        finally
        {
            CloseClipboard();
        }
    }

    private static bool SetTextLinux(string text)
    {
        using var process = new System.Diagnostics.Process
        {
            StartInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "xclip",
                Arguments = "-selection clipboard",
                RedirectStandardInput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };
        process.Start();
        process.StandardInput.Write(text);
        process.StandardInput.Close();
        process.WaitForExit();
        return process.ExitCode == 0;
    }

    private static string? GetTextLinux()
    {
        using var process = new System.Diagnostics.Process
        {
            StartInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "xclip",
                Arguments = "-selection clipboard -o",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };
        process.Start();
        var result = process.StandardOutput.ReadToEnd();
        process.WaitForExit();
        return process.ExitCode == 0 ? result : null;
    }

    private static bool ContainsTextLinux()
    {
        using var process = new System.Diagnostics.Process
        {
            StartInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "xclip",
                Arguments = "-selection clipboard -o",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };
        process.Start();
        process.WaitForExit();
        return process.ExitCode == 0;
    }

    private static bool SetTextMacOS(string text)
    {
        using var process = new System.Diagnostics.Process
        {
            StartInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "pbcopy",
                RedirectStandardInput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };
        process.Start();
        process.StandardInput.Write(text);
        process.StandardInput.Close();
        process.WaitForExit();
        return process.ExitCode == 0;
    }

    private static string? GetTextMacOS()
    {
        using var process = new System.Diagnostics.Process
        {
            StartInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "pbpaste",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };
        process.Start();
        var result = process.StandardOutput.ReadToEnd();
        process.WaitForExit();
        return process.ExitCode == 0 ? result : null;
    }

    private static bool ContainsTextMacOS()
    {
        using var process = new System.Diagnostics.Process
        {
            StartInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "pbpaste",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };
        process.Start();
        process.WaitForExit();
        return process.ExitCode == 0;
    }

    [DllImport("user32.dll")]
    private static extern bool OpenClipboard(IntPtr hWndNewOwner);

    [DllImport("user32.dll")]
    private static extern bool CloseClipboard();

    [DllImport("user32.dll")]
    private static extern bool EmptyClipboard();

    [DllImport("user32.dll")]
    private static extern IntPtr SetClipboardData(uint uFormat, IntPtr hMem);

    [DllImport("user32.dll")]
    private static extern IntPtr GetClipboardData(uint uFormat);

    [DllImport("user32.dll")]
    private static extern bool IsClipboardFormatAvailable(uint format);
}