using System.Runtime.InteropServices;
using System.Diagnostics;

namespace PCL_CE.Neo.Platform.Windows;

public class WindowsPlatformService : Core.Abstractions.IPlatformService
{
    public string PlatformName => "Windows";

    public string OSVersion => RuntimeInformation.OSDescription;

    public string Architecture => RuntimeInformation.OSArchitecture.ToString();

    public void OpenUrl(string url)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }
        catch { }
    }

    public void OpenFolder(string path)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments = path,
                UseShellExecute = true
            });
        }
        catch { }
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
