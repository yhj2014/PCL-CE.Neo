using System.Runtime.InteropServices;

namespace PCL_CE.Neo.Platform.Linux;

public class LinuxPlatformService : Core.Abstractions.IPlatformService
{
    public string PlatformName
    {
        get
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return "Windows";
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                return "macOS";
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return "Linux";
            return "Unknown";
        }
    }

    public string OSVersion => RuntimeInformation.OSDescription;

    public string Architecture => RuntimeInformation.OSArchitecture.ToString();

    public void OpenUrl(string url)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            System.Diagnostics.Process.Start("open", url);
        }
        else
        {
            System.Diagnostics.Process.Start("xdg-open", url);
        }
    }

    public void OpenFolder(string path)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments = path,
                UseShellExecute = true
            });
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            System.Diagnostics.Process.Start("open", path);
        }
        else
        {
            System.Diagnostics.Process.Start("xdg-open", path);
        }
    }

    public string GetLocalApplicationDataPath()
    {
        return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
    }

    public string GetTempPath()
    {
        return Path.GetTempPath();
    }

    public string GetGameDataPath()
    {
        var basePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        return Path.Combine(basePath, "PCL-CE.Neo", "GameData");
    }
}
