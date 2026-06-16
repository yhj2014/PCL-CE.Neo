using System;
using System.Diagnostics;
using System.IO;
using PCL_CE.Neo.Core.Logging;

namespace PCL_CE.Neo.Core.Utils.Tools;

public class DependencyCheckService
{
    private const string ModuleName = "DependencyCheckService";

    public static bool CheckJavaInstallation()
    {
        try
        {
            var process = new ProcessStartInfo
            {
                FileName = "java",
                Arguments = "-version",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var proc = Process.Start(process);
            if (proc == null) return false;

            proc.WaitForExit(5000);
            return proc.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }

    public static bool CheckNetworkConnectivity()
    {
        try
        {
            var host = OperatingSystem.IsWindows() ? "www.google.com" : "8.8.8.8";
            var process = new ProcessStartInfo
            {
                FileName = OperatingSystem.IsWindows() ? "ping" : "ping",
                Arguments = OperatingSystem.IsWindows() ? $"-n 1 -w 3000 {host}" : $"-c 1 -W 3 {host}",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var proc = Process.Start(process);
            if (proc == null) return false;

            proc.WaitForExit(5000);
            return proc.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }

    public static bool CheckDiskSpace(string path, long requiredBytes)
    {
        try
        {
            var driveInfo = new DriveInfo(Path.GetPathRoot(path) ?? "/");
            return driveInfo.AvailableFreeSpace >= requiredBytes;
        }
        catch
        {
            return false;
        }
    }

    public static bool CheckMinecraftDirectory(string path)
    {
        try
        {
            if (!Directory.Exists(path)) return false;

            var requiredFiles = new[] { "versions", "assets", "libraries" };
            return requiredFiles.All(dir => Directory.Exists(Path.Combine(path, dir)));
        }
        catch
        {
            return false;
        }
    }

    public static bool CheckDotNetRuntime()
    {
        try
        {
            var process = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = "--version",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var proc = Process.Start(process);
            if (proc == null) return false;

            proc.WaitForExit(5000);
            if (proc.ExitCode != 0) return false;

            var version = proc.StandardOutput.ReadToEnd().Trim();
            return Version.TryParse(version, out var v) && v.Major >= 8;
        }
        catch
        {
            return false;
        }
    }

    public static DependencyStatus CheckAllDependencies(string minecraftPath)
    {
        var status = new DependencyStatus();

        try
        {
            status.HasJava = CheckJavaInstallation();
            status.HasNetwork = CheckNetworkConnectivity();
            status.HasDiskSpace = CheckDiskSpace(minecraftPath, 10L * 1024 * 1024 * 1024);
            status.HasValidMinecraftDir = CheckMinecraftDirectory(minecraftPath);
            status.HasDotNetRuntime = CheckDotNetRuntime();
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, ModuleName, "依赖检查失败");
        }

        return status;
    }
}

public class DependencyStatus
{
    public bool HasJava { get; set; }
    public bool HasNetwork { get; set; }
    public bool HasDiskSpace { get; set; }
    public bool HasValidMinecraftDir { get; set; }
    public bool HasDotNetRuntime { get; set; }

    public bool AllDependenciesMet => 
        HasJava && HasNetwork && HasDiskSpace && HasValidMinecraftDir && HasDotNetRuntime;
}