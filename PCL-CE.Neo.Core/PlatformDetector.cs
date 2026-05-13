using PCL_CE.Neo.Core.Abstractions;

namespace PCL_CE.Neo.Core;

public static class PlatformDetector
{
    public static string CurrentPlatform { get; }
    public static bool IsWindows { get; }
    public static bool IsMacOS { get; }
    public static bool IsLinux { get; }

    static PlatformDetector()
    {
        CurrentPlatform = OperatingSystem.IsWindows() ? "Windows"
            : OperatingSystem.IsMacOS() ? "macOS"
            : OperatingSystem.IsLinux() ? "Linux"
            : "Unknown";

        IsWindows = CurrentPlatform == "Windows";
        IsMacOS = CurrentPlatform == "macOS";
        IsLinux = CurrentPlatform == "Linux";
    }

    // public static IPlatformService CreatePlatformService()
    // {
    //     return CurrentPlatform switch
    //     {
    //         "Windows" => new Platform.Windows.WindowsPlatformService(),
    //         "macOS" => new Platform.macOS.MacOSPlatformService(),
    //         "Linux" => new Platform.Linux.LinuxPlatformService(),
    //         _ => throw new NotSupportedException($"不支持的平台: {CurrentPlatform}")
    //     };
    // }
    //
    // public static IJavaScanner CreateJavaScanner()
    // {
    //     return CurrentPlatform switch
    //     {
    //         "Windows" => new Platform.Windows.WindowsJavaScanner(),
    //         "macOS" => new Platform.macOS.MacOSJavaScanner(),
    //         "Linux" => new Platform.Linux.LinuxJavaScanner(),
    //         _ => throw new NotSupportedException($"不支持的平台: {CurrentPlatform}")
    //     };
    // }

    public static string GetLineEnding()
    {
        return CurrentPlatform switch
        {
            "Windows" => "\r\n",
            "macOS" or "Linux" => "\n",
            _ => "\n"
        };
    }

    public static string GetPathSeparator()
    {
        return CurrentPlatform switch
        {
            "Windows" => ";",
            "macOS" or "Linux" => ":",
            _ => ":"
        };
    }

    public static string NormalizePath(string path)
    {
        if (string.IsNullOrEmpty(path)) return path;

        return CurrentPlatform switch
        {
            "Windows" => path.Replace('/', '\\'),
            "macOS" or "Linux" => path.Replace('\\', '/'),
            _ => path
        };
    }
}
