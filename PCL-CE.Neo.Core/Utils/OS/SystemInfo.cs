using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using PCL_CE.Neo.Core.Logging;

namespace PCL_CE.Neo.Core.Utils.OS;

public static class SystemInfo
{
    private static readonly object _lock = new();

    public static string CPUName = string.Empty;
    public static List<GPUInfo> GPUs = new();
    public static long SystemMemorySize;
    public static string OSInfo = RuntimeInformation.OSDescription + " " + Environment.OSVersion.Version;

    public class GPUInfo
    {
        public string Name = string.Empty;
        public string DriverVersion = string.Empty;
        public long Memory;
    }

    public static void GetSystemInfo()
    {
        lock (_lock)
        {
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    _GetSystemInfoWindows();
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    _GetSystemInfoLinux();
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                    _GetSystemInfoMacOS();

                LogWrapper.Info("SystemInfo", "已获取系统环境信息");
            }
            catch (Exception ex)
            {
                LogWrapper.Warn(ex, "SystemInfo", "获取系统信息时出错");
            }
        }
    }

    private static void _GetSystemInfoWindows()
    {
        try
        {
            using var cpuSearcher = new System.Management.ManagementObjectSearcher(@"root\CIMV2", "SELECT * FROM Win32_Processor");
            foreach (System.Management.ManagementObject queryObj in cpuSearcher.Get())
            {
                CPUName = queryObj["Name"]?.ToString()?.Trim() ?? string.Empty;
                break;
            }

            GPUs.Clear();
            using var gpuSearcher = new System.Management.ManagementObjectSearcher(@"root\CIMV2", "SELECT * FROM Win32_VideoController");
            foreach (System.Management.ManagementObject queryObj in gpuSearcher.Get())
            {
                var gpuInfo = new GPUInfo();
                gpuInfo.Name = queryObj["Name"]?.ToString() ?? string.Empty;
                if (queryObj["AdapterRAM"] is not null and not DBNull)
                    gpuInfo.Memory = Convert.ToInt64(queryObj["AdapterRAM"]) / (1024 * 1024);
                gpuInfo.DriverVersion = queryObj["DriverVersion"]?.ToString() ?? string.Empty;
                GPUs.Add(gpuInfo);
            }

            using var memSearcher = new System.Management.ManagementObjectSearcher(@"root\CIMV2", "SELECT * FROM Win32_ComputerSystem");
            foreach (System.Management.ManagementObject queryObj in memSearcher.Get())
            {
                if (queryObj["TotalPhysicalMemory"] is not null and not DBNull)
                    SystemMemorySize = Convert.ToInt64(queryObj["TotalPhysicalMemory"]) / (1024 * 1024);
                break;
            }
        }
        catch (Exception ex)
        {
            LogWrapper.Warn(ex, "SystemInfo", "获取 Windows 系统信息时出错");
        }
    }

    private static void _GetSystemInfoLinux()
    {
        try
        {
            if (System.IO.File.Exists("/proc/cpuinfo"))
            {
                var cpuInfo = System.IO.File.ReadAllText("/proc/cpuinfo");
                var lines = cpuInfo.Split('\n');
                foreach (var line in lines)
                {
                    if (line.StartsWith("model name"))
                    {
                        CPUName = line.Split(':')[1].Trim();
                        break;
                    }
                }
            }

            if (System.IO.File.Exists("/proc/meminfo"))
            {
                var memInfo = System.IO.File.ReadAllText("/proc/meminfo");
                var lines = memInfo.Split('\n');
                foreach (var line in lines)
                {
                    if (line.StartsWith("MemTotal"))
                    {
                        var parts = line.Split(':');
                        if (parts.Length > 1)
                        {
                            var value = parts[1].Trim().Split()[0];
                            if (long.TryParse(value, out var kb))
                                SystemMemorySize = kb / 1024;
                        }
                        break;
                    }
                }
            }

            GPUs.Clear();
            var gpuDirs = System.IO.Directory.GetDirectories("/sys/class/drm", "card*");
            foreach (var gpuDir in gpuDirs)
            {
                var gpuInfo = new GPUInfo();
                var namePath = System.IO.Path.Combine(gpuDir, "device", "product");
                if (System.IO.File.Exists(namePath))
                {
                    var nameHex = System.IO.File.ReadAllText(namePath).Trim();
                    if (uint.TryParse(nameHex, System.Globalization.NumberStyles.HexNumber, null, out var nameId))
                    {
                        gpuInfo.Name = $"GPU {nameId}";
                    }
                }
                var memPath = System.IO.Path.Combine(gpuDir, "device", "mem_info_vram_total");
                if (System.IO.File.Exists(memPath))
                {
                    if (long.TryParse(System.IO.File.ReadAllText(memPath).Trim(), out var bytes))
                        gpuInfo.Memory = bytes / (1024 * 1024);
                }
                if (!string.IsNullOrEmpty(gpuInfo.Name) || gpuInfo.Memory > 0)
                    GPUs.Add(gpuInfo);
            }
        }
        catch (Exception ex)
        {
            LogWrapper.Warn(ex, "SystemInfo", "获取 Linux 系统信息时出错");
        }
    }

    private static void _GetSystemInfoMacOS()
    {
        try
        {
            using var process = new System.Diagnostics.Process();
            process.StartInfo.FileName = "/usr/sbin/sysctl";
            process.StartInfo.Arguments = "-n machdep.cpu.brand_string";
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.UseShellExecute = false;
            process.Start();
            CPUName = process.StandardOutput.ReadToEnd().Trim();
            process.WaitForExit();

            process.StartInfo.Arguments = "-n hw.memsize";
            process.Start();
            var memSize = process.StandardOutput.ReadToEnd().Trim();
            if (long.TryParse(memSize, out var bytes))
                SystemMemorySize = bytes / (1024 * 1024);
            process.WaitForExit();

            GPUs.Clear();
            process.StartInfo.Arguments = "-n machdep.cpu.brand_string";
            process.Start();
            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            var gpuInfo = new GPUInfo();
            gpuInfo.Name = "Apple GPU";
            GPUs.Add(gpuInfo);
        }
        catch (Exception ex)
        {
            LogWrapper.Warn(ex, "SystemInfo", "获取 macOS 系统信息时出错");
        }
    }
}