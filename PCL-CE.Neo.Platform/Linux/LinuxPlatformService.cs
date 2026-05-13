using System.Diagnostics;
using System.Runtime.InteropServices;
using PCL_CE.Neo.Core.Abstractions;

namespace PCL_CE.Neo.Platform.Linux;

public class LinuxPlatformService : IPlatformService
{
    public string PlatformName => "Linux";
    public string OSVersion => Environment.OSVersion.VersionString;
    public string Architecture => RuntimeInformation.ProcessArchitecture.ToString().ToLowerInvariant();

    public void OpenUrl(string url)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = "xdg-open",
            Arguments = url,
            UseShellExecute = false
        });
    }

    public void OpenFolder(string path)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = "xdg-open",
            Arguments = path,
            UseShellExecute = false
        });
    }

    public string GetLocalApplicationDataPath()
    {
        var home = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        return Path.Combine(home, ".local", "share", "PCL");
    }

    public string GetTempPath()
    {
        return Path.GetTempPath();
    }

    public string GetGameDataPath()
    {
        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            ".minecraft"
        );
    }
}
