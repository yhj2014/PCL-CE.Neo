using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using PCL_CE.Neo.Core.Logging;

namespace PCL_CE.Neo.Core.Utils.Tools;

public class MemSwapService
{
    private const string ModuleName = "MemSwapService";

    public static long GetAvailableMemory()
    {
        try
        {
            if (OperatingSystem.IsWindows())
            {
                return _GetWindowsAvailableMemory();
            }
            else if (OperatingSystem.IsLinux())
            {
                return _GetLinuxAvailableMemory();
            }
            else if (OperatingSystem.IsMacOS())
            {
                return _GetMacOSAvailableMemory();
            }

            return 0;
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, ModuleName, "获取可用内存失败");
            return 0;
        }
    }

    private static long _GetWindowsAvailableMemory()
    {
        try
        {
            var process = new ProcessStartInfo
            {
                FileName = "wmic",
                Arguments = "OS get FreePhysicalMemory /Value",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var proc = Process.Start(process);
            if (proc == null) return 0;

            proc.WaitForExit();
            var output = proc.StandardOutput.ReadToEnd();

            var lines = output.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                if (line.StartsWith("FreePhysicalMemory=", StringComparison.OrdinalIgnoreCase))
                {
                    if (long.TryParse(line.Split('=')[1].Trim(), out var kb))
                        return kb * 1024;
                }
            }

            return 0;
        }
        catch
        {
            return 0;
        }
    }

    private static long _GetLinuxAvailableMemory()
    {
        try
        {
            var memInfo = File.ReadAllText("/proc/meminfo");
            var lines = memInfo.Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                if (line.StartsWith("MemAvailable:"))
                {
                    var parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 2 && long.TryParse(parts[1], out var kb))
                        return kb * 1024;
                }
            }

            foreach (var line in lines)
            {
                if (line.StartsWith("MemFree:"))
                {
                    var parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 2 && long.TryParse(parts[1], out var kb))
                        return kb * 1024;
                }
            }

            return 0;
        }
        catch
        {
            return 0;
        }
    }

    private static long _GetMacOSAvailableMemory()
    {
        try
        {
            var process = new ProcessStartInfo
            {
                FileName = "vm_stat",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var proc = Process.Start(process);
            if (proc == null) return 0;

            proc.WaitForExit();
            var output = proc.StandardOutput.ReadToEnd();

            var lines = output.Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                if (line.Contains("free"))
                {
                    var parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 2 && long.TryParse(parts[1].Replace(".", ""), out var pages))
                        return pages * 4096;
                }
            }

            return 0;
        }
        catch
        {
            return 0;
        }
    }

    public static bool IsMemoryLow(long thresholdBytes = 1024 * 1024 * 1024)
    {
        var available = GetAvailableMemory();
        return available > 0 && available < thresholdBytes;
    }
}