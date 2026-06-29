using System;
using System.Runtime.InteropServices;

namespace PCL_CE.Neo.Core.Utils;

public static class EnvironmentHelper
{
    public static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
    public static bool IsLinux => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
    public static bool IsMacOS => RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

    public static string GetAppDataPath()
    {
        if (IsWindows)
            return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        
        if (IsMacOS)
            return Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/Library/Application Support";
        
        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var xdgData = Environment.GetEnvironmentVariable("XDG_DATA_HOME");
        
        return string.IsNullOrEmpty(xdgData) ? $"{home}/.local/share" : xdgData;
    }

    public static string GetCachePath()
    {
        if (IsWindows)
            return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        
        if (IsMacOS)
            return Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/Library/Caches";
        
        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var xdgCache = Environment.GetEnvironmentVariable("XDG_CACHE_HOME");
        
        return string.IsNullOrEmpty(xdgCache) ? $"{home}/.cache" : xdgCache;
    }

    public static string GetTempPath()
    {
        return Path.GetTempPath();
    }

    public static string GetHomePath()
    {
        return Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
    }

    public static string GetDesktopPath()
    {
        return Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
    }

    public static string GetDocumentsPath()
    {
        return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
    }

    public static string GetDownloadsPath()
    {
        if (IsWindows)
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
        
        if (IsMacOS)
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
        
        var xdgDownloads = Environment.GetEnvironmentVariable("XDG_DOWNLOAD_DIR");
        if (!string.IsNullOrEmpty(xdgDownloads))
            return xdgDownloads;
        
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
    }

    public static bool Is64BitOperatingSystem => Environment.Is64BitOperatingSystem;
    public static bool Is64BitProcess => Environment.Is64BitProcess;

    public static string GetProcessArchitecture()
    {
        return Is64BitProcess ? "x64" : "x86";
    }

    public static string GetOperatingSystemVersion()
    {
        return Environment.OSVersion.ToString();
    }

    public static string GetFrameworkDescription()
    {
        return RuntimeInformation.FrameworkDescription;
    }

    public static string GetRuntimeIdentifier()
    {
        return RuntimeInformation.RuntimeIdentifier;
    }

    public static int GetProcessorCount()
    {
        return Environment.ProcessorCount;
    }

    public static long GetTotalMemory()
    {
        return GC.GetTotalMemory(false);
    }

    public static string GetEnvironmentVariable(string name, string defaultValue = "")
    {
        return Environment.GetEnvironmentVariable(name) ?? defaultValue;
    }

    public static void SetEnvironmentVariable(string name, string value)
    {
        Environment.SetEnvironmentVariable(name, value);
    }

    public static bool HasEnvironmentVariable(string name)
    {
        return !string.IsNullOrEmpty(Environment.GetEnvironmentVariable(name));
    }

    public static bool IsRunningAsAdministrator()
    {
        try
        {
            if (IsWindows)
            {
                using var identity = System.Security.Principal.WindowsIdentity.GetCurrent();
                var principal = new System.Security.Principal.WindowsPrincipal(identity);
                return principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
            }

            if (IsLinux || IsMacOS)
            {
                return Environment.GetUid() == 0;
            }

            return false;
        }
        catch (Exception ex)
        {
            LogWrapper.Warn(ex, "Failed to check if running as administrator");
            return false;
        }
    }

    public static string GetPlatformName()
    {
        if (IsWindows)
            return "Windows";
        if (IsMacOS)
            return "macOS";
        if (IsLinux)
            return "Linux";
        return "Unknown";
    }

    public static string GetPlatformShortName()
    {
        if (IsWindows)
            return "win";
        if (IsMacOS)
            return "mac";
        if (IsLinux)
            return "linux";
        return "unknown";
    }
}