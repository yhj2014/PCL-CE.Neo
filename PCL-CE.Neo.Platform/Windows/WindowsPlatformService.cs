using System.Diagnostics;
using System.Runtime.InteropServices;
using PCL.CE.Neo.Core.Abstractions;

namespace PCL.CE.Neo.Platform.Windows;

public class WindowsPlatformService : IPlatformService
{
    public string PlatformName => "Windows";
    public string OSVersion => Environment.OSVersion.VersionString;
    public string Architecture => RuntimeInformation.ProcessArchitecture.ToString().ToLowerInvariant();

    public void OpenUrl(string url)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = url,
            UseShellExecute = true
        });
    }

    public void OpenFolder(string path)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = "explorer.exe",
            Arguments = $"\"{path}\"",
            UseShellExecute = true
        });
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
        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            ".minecraft"
        );
    }
}
