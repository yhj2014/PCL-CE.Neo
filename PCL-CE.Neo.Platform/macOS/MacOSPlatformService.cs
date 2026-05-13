using System.Diagnostics;
using System.Runtime.InteropServices;
using PCL_CE.Neo.Core.Abstractions;

namespace PCL_CE.Neo.Platform.macOS;

public class MacOSPlatformService : IPlatformService
{
    public string PlatformName => "macOS";
    public string OSVersion => Environment.OSVersion.VersionString;
    public string Architecture => RuntimeInformation.ProcessArchitecture.ToString().ToLowerInvariant();

    public void OpenUrl(string url)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = "open",
            Arguments = url,
            UseShellExecute = false
        });
    }

    public void OpenFolder(string path)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = "open",
            Arguments = path,
            UseShellExecute = false
        });
    }

    public string GetLocalApplicationDataPath()
    {
        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "PCL"
        );
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
