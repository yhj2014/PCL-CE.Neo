using System.Runtime.InteropServices;
using System.Diagnostics;

namespace PCL_CE.Neo.Platform.macOS;

public class MacOSPlatformService : Core.Abstractions.IPlatformService
{
    public string PlatformName => "macOS";

    public string OSVersion => RuntimeInformation.OSDescription;

    public string Architecture => RuntimeInformation.OSArchitecture.ToString();

    public void OpenUrl(string url)
    {
        try
        {
            Process.Start("open", url);
        }
        catch { }
    }

    public void OpenFolder(string path)
    {
        try
        {
            Process.Start("open", path);
        }
        catch { }
    }

    public string GetLocalApplicationDataPath()
    {
        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        return Path.Combine(home, "Library", "Application Support");
    }

    public string GetTempPath()
    {
        return Path.GetTempPath();
    }

    public string GetGameDataPath()
    {
        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        return Path.Combine(home, "Library", "Application Support", "PCL-CE.Neo", "GameData");
    }
}
