using System.Runtime.InteropServices;

namespace PCL_CE.Neo.Core.Utils.OS;

public static class SystemInfo
{
    public static OSPlatform GetOSPlatform()
    {
        if (OperatingSystem.IsWindows())
            return OSPlatform.Windows;
        if (OperatingSystem.IsMacOS())
            return OSPlatform.OSX;
        if (OperatingSystem.IsLinux())
            return OSPlatform.Linux;
        return OSPlatform.Create("UNKNOWN");
    }

    public static bool IsWindows => OperatingSystem.IsWindows();
    public static bool IsMacOS => OperatingSystem.IsMacOS();
    public static bool IsLinux => OperatingSystem.IsLinux();

    public static string GetOSVersion()
    {
        return Environment.OSVersion.VersionString;
    }

    public static string GetOSName()
    {
        if (IsWindows)
            return "Windows";
        if (IsMacOS)
            return "macOS";
        if (IsLinux)
            return "Linux";
        return "Unknown";
    }

    public static string GetArchitecture()
    {
        return RuntimeInformation.ProcessArchitecture.ToString();
    }

    public static bool Is64Bit => Environment.Is64BitOperatingSystem;

    public static string GetProcessorCount()
    {
        return Environment.ProcessorCount.ToString();
    }

    public static long GetTotalMemoryBytes()
    {
        return GC.GetTotalMemory(false);
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
        return Environment.UserDomainName;
    }

    public static string GetCurrentDirectory()
    {
        return Environment.CurrentDirectory;
    }

    public static string GetCommandLine()
    {
        return Environment.CommandLine;
    }

    public static int GetProcessId()
    {
        return Environment.ProcessId;
    }

    public static int GetThreadId()
    {
        return Environment.CurrentManagedThreadId;
    }

    public static bool IsUserInteractive()
    {
        return Environment.UserInteractive;
    }

    public static string GetEnvironmentVariable(string name)
    {
        return Environment.GetEnvironmentVariable(name) ?? string.Empty;
    }

    public static void SetEnvironmentVariable(string name, string value)
    {
        Environment.SetEnvironmentVariable(name, value);
    }

    public static IDictionary<string, string> GetEnvironmentVariables()
    {
        var variables = Environment.GetEnvironmentVariables();
        var dict = new Dictionary<string, string>();
        foreach (System.Collections.DictionaryEntry entry in variables)
        {
            dict[(string)entry.Key] = entry.Value?.ToString() ?? string.Empty;
        }
        return dict;
    }

    public static void Exit(int exitCode)
    {
        Environment.Exit(exitCode);
    }
}