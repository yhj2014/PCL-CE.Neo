using System.Runtime.InteropServices;
using System.Diagnostics;

namespace PCL_CE.Neo.Platform.Linux;

public class LinuxPlatformService : Core.Abstractions.IPlatformService
{
    public string PlatformName => "Linux";

    public string OSVersion => RuntimeInformation.OSDescription;

    public string Architecture => RuntimeInformation.OSArchitecture.ToString();

    public void OpenUrl(string url)
    {
        try
        {
            Process.Start("xdg-open", url);
        }
        catch { }
    }

    public void OpenFolder(string path)
    {
        try
        {
            Process.Start("xdg-open", path);
        }
        catch { }
    }

    public string GetLocalApplicationDataPath()
    {
        var home = Environment.GetEnvironmentVariable("XDG_DATA_HOME");
        if (string.IsNullOrEmpty(home))
        {
            home = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local", "share");
        }
        return home;
    }

    public string GetTempPath()
    {
        return Path.GetTempPath();
    }

    public string GetGameDataPath()
    {
        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        return Path.Combine(home, ".config", "PCL-CE.Neo", "GameData");
    }
}
