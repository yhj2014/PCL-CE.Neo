using System;
using System.Runtime.InteropServices;

namespace PCL_CE.Neo.Core.Utils;

public static class RuntimeDetection
{
    private static readonly Lazy<bool> _isWindows = new(() => RuntimeInformation.IsOSPlatform(OSPlatform.Windows));
    private static readonly Lazy<bool> _isLinux = new(() => RuntimeInformation.IsOSPlatform(OSPlatform.Linux));
    private static readonly Lazy<bool> _isMacOS = new(() => RuntimeInformation.IsOSPlatform(OSPlatform.OSX));
    private static readonly Lazy<bool> _isFreeBSD = new(() => RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD));

    private static readonly Lazy<Architecture> _processArchitecture = new(() => RuntimeInformation.ProcessArchitecture);
    private static readonly Lazy<Architecture> _osArchitecture = new(() => RuntimeInformation.OSArchitecture);

    private static readonly Lazy<string> _osDescription = new(() => RuntimeInformation.OSDescription);
    private static readonly Lazy<string> _frameworkDescription = new(() => RuntimeInformation.FrameworkDescription);

    public static bool IsWindows => _isWindows.Value;
    public static bool IsLinux => _isLinux.Value;
    public static bool IsMacOS => _isMacOS.Value;
    public static bool IsFreeBSD => _isFreeBSD.Value;

    public static Architecture ProcessArchitecture => _processArchitecture.Value;
    public static Architecture OSArchitecture => _osArchitecture.Value;

    public static string OSDescription => _osDescription.Value;
    public static string FrameworkDescription => _frameworkDescription.Value;

    public static bool Is64BitProcess => Environment.Is64BitProcess;
    public static bool Is64BitOperatingSystem => Environment.Is64BitOperatingSystem;

    public static OSPlatform GetCurrentOSPlatform()
    {
        if (IsWindows) return OSPlatform.Windows;
        if (IsLinux) return OSPlatform.Linux;
        if (IsMacOS) return OSPlatform.OSX;
        if (IsFreeBSD) return OSPlatform.FreeBSD;
        throw new PlatformNotSupportedException("Unsupported operating system");
    }

    public static string GetOSName()
    {
        if (IsWindows) return "Windows";
        if (IsLinux) return "Linux";
        if (IsMacOS) return "macOS";
        if (IsFreeBSD) return "FreeBSD";
        return "Unknown";
    }

    public static bool IsRunningOnMono()
    {
        return Type.GetType("Mono.Runtime") != null;
    }

    public static bool IsRunningOnCoreCLR()
    {
        return FrameworkDescription.Contains("CoreCLR", StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsRunningOnNetFramework()
    {
        return FrameworkDescription.Contains(".NET Framework", StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsRunningOnNetCore()
    {
        return FrameworkDescription.Contains(".NET Core", StringComparison.OrdinalIgnoreCase) ||
               FrameworkDescription.Contains(".NET 5", StringComparison.OrdinalIgnoreCase) ||
               FrameworkDescription.Contains(".NET 6", StringComparison.OrdinalIgnoreCase) ||
               FrameworkDescription.Contains(".NET 7", StringComparison.OrdinalIgnoreCase) ||
               FrameworkDescription.Contains(".NET 8", StringComparison.OrdinalIgnoreCase) ||
               FrameworkDescription.Contains(".NET 9", StringComparison.OrdinalIgnoreCase) ||
               FrameworkDescription.Contains(".NET 10", StringComparison.OrdinalIgnoreCase);
    }
}