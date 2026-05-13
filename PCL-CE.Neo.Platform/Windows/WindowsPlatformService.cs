using System.Diagnostics;
using System.Runtime.InteropServices;
using PCL_CE.Neo.Core.Abstractions;

namespace PCL_CE.Neo.Platform.Windows;

public class WindowsPlatformService : IPlatformService
{
    public string PlatformName => "Windows";
    public string OSVersion => GetWindowsVersion();
    public string Architecture => RuntimeInformation.ProcessArchitecture.ToString().ToLowerInvariant();

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
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to open URL: {ex.Message}");
        }
    }

    public void OpenFolder(string path)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments = $"\"{path}\"",
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to open folder: {ex.Message}");
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
        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            ".minecraft"
        );
    }

    private string GetWindowsVersion()
    {
        try
        {
            // 尝试获取更详细的 Windows 版本信息
            var osVersion = Environment.OSVersion;
            var version = $"{osVersion.Platform} {osVersion.Version}";

            // 尝试获取 Windows 10/11 友好名称
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                try
                {
                    using var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(
                        @"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
                    if (key != null)
                    {
                        var productName = key.GetValue("ProductName")?.ToString();
                        var releaseId = key.GetValue("ReleaseId")?.ToString();
                        if (!string.IsNullOrEmpty(productName))
                        {
                            return !string.IsNullOrEmpty(releaseId)
                                ? $"{productName} {releaseId}"
                                : productName;
                        }
                    }
                }
                catch
                {
                    // 忽略注册表访问错误
                }
            }
            return version;
        }
        catch
        {
            return Environment.OSVersion.VersionString;
        }
    }
}
