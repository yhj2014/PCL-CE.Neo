using System.Runtime.InteropServices;

namespace PCL_CE.Neo.Core.Utils.Platform;

public static class SystemInfo
{
    public static OSPlatform GetOSPlatform()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return OSPlatform.Windows;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return OSPlatform.Linux;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return OSPlatform.OSX;
        return OSPlatform.Create("UNKNOWN");
    }

    public static string GetOSDescription()
    {
        return RuntimeInformation.OSDescription;
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

    public static bool Is64BitProcess()
    {
        return Environment.Is64BitProcess;
    }

    public static bool Is64BitOperatingSystem()
    {
        return Environment.Is64BitOperatingSystem;
    }

    public static string GetMachineName()
    {
        return Environment.MachineName;
    }

    public static string GetUserName()
    {
        return Environment.UserName;
    }

    public static string GetUserDomainName()
    {
        try
        {
            return Environment.UserDomainName;
        }
        catch
        {
            return string.Empty;
        }
    }
}