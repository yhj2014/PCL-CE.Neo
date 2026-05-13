using PCL_CE.Neo.Core.Abstractions;
using System.Reflection;

namespace PCL_CE.Neo.Core;

public static class PlatformDetector
{
    public static string CurrentPlatform { get; }
    public static bool IsWindows { get; }
    public static bool IsMacOS { get; }
    public static bool IsLinux { get; }

    private static readonly Dictionary<string, (string ServiceName, string ScannerName)> _platformMap = new()
    {
        ["Windows"] = ("WindowsPlatformService", "WindowsJavaScanner"),
        ["macOS"] = ("MacOSPlatformService", "MacOSJavaScanner"),
        ["Linux"] = ("LinuxPlatformService", "LinuxJavaScanner")
    };

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

    public static IPlatformService CreatePlatformService()
    {
        if (CurrentPlatform == "Unknown")
            throw new NotSupportedException("不支持的操作系统");

        var (serviceName, _) = _platformMap[CurrentPlatform];
        return CreateInstance<IPlatformService>($"PCL_CE.Neo.Platform.{CurrentPlatform}.{serviceName}", serviceName);
    }

    public static IJavaScanner CreateJavaScanner()
    {
        if (CurrentPlatform == "Unknown")
            throw new NotSupportedException("不支持的操作系统");

        var (_, scannerName) = _platformMap[CurrentPlatform];
        return CreateInstance<IJavaScanner>($"PCL_CE.Neo.Platform.{CurrentPlatform}.{scannerName}", scannerName);
    }

    private static T CreateInstance<T>(string fullTypeName, string shortName) where T : class
    {
        var assemblyName = $"PCL-CE.Neo.Platform.{CurrentPlatform}";
        try
        {
            var assembly = Assembly.Load(assemblyName);
            var type = assembly.GetType($"{fullTypeName}");
            if (type == null)
            {
                throw new TypeLoadException($"无法找到类型: {fullTypeName}");
            }
            return Activator.CreateInstance(type) as T
                ?? throw new InvalidOperationException($"无法创建类型实例: {fullTypeName}");
        }
        catch (Exception ex)
        {
            throw new NotSupportedException($"无法加载平台服务 {shortName}: {ex.Message}", ex);
        }
    }

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
