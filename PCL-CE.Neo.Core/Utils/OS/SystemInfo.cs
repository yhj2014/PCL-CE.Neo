using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;

namespace PCL_CE.Neo.Core.Utils.OS;

public class SystemInfo
{
    private readonly ILogger<SystemInfo> _logger;
    private readonly object _lock = new();

    public string CPUName { get; private set; } = string.Empty;
    public List<GPUInfo> GPUs { get; private set; } = [];
    public long SystemMemorySize { get; private set; }
    public string OSInfo { get; private set; } = string.Empty;

    public SystemInfo(ILogger<SystemInfo> logger)
    {
        _logger = logger;
        OSInfo = RuntimeInformation.OSDescription + " " + Environment.OSVersion.Version;
        SystemMemorySize = GetSystemMemorySizeMb();
    }

    public class GPUInfo
    {
        public string Name { get; set; } = string.Empty;
        public string DriverVersion { get; set; } = string.Empty;
        public long Memory { get; set; }
    }

    public void GetSystemInfo()
    {
        lock (_lock)
        {
            CPUName = GetCpuName();
            GPUs = GetGpuInfo();
            SystemMemorySize = GetSystemMemorySizeMb();
            _logger.LogInformation("已获取系统环境信息");
        }
    }

    private string GetCpuName()
    {
        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return GetWindowsCpuName();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return GetLinuxCpuName();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return GetMacCpuName();
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "获取 CPU 信息时出错");
        }
        return "Unknown CPU";
    }

    private List<GPUInfo> GetGpuInfo()
    {
        var gpus = new List<GPUInfo>();
        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                gpus = GetWindowsGpuInfo();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                gpus = GetLinuxGpuInfo();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                gpus = GetMacGpuInfo();
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "获取 GPU 信息时出错");
        }
        return gpus;
    }

    private long GetSystemMemorySizeMb()
    {
        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return GetWindowsSystemMemorySizeMb();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return GetLinuxSystemMemorySizeMb();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return GetMacSystemMemorySizeMb();
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "获取系统内存信息时出错");
        }
        return 0;
    }

    private string GetWindowsCpuName()
    {
        try
        {
            using var searcher = new System.Management.ManagementObjectSearcher(@"root\CIMV2", "SELECT * FROM Win32_Processor");
            foreach (System.Management.ManagementObject queryObj in searcher.Get())
            {
                return queryObj["Name"]?.ToString()?.Trim() ?? "Unknown CPU";
            }
        }
        catch
        {
        }
        return "Unknown CPU";
    }

    private List<GPUInfo> GetWindowsGpuInfo()
    {
        var gpus = new List<GPUInfo>();
        try
        {
            using var searcher = new System.Management.ManagementObjectSearcher(@"root\CIMV2", "SELECT * FROM Win32_VideoController");
            foreach (System.Management.ManagementObject queryObj in searcher.Get())
            {
                var gpuInfo = new GPUInfo();
                if (queryObj["Name"] is not null)
                    gpuInfo.Name = queryObj["Name"].ToString() ?? string.Empty;
                if (queryObj["AdapterRAM"] is not null and not DBNull)
                    gpuInfo.Memory = Convert.ToInt64(queryObj["AdapterRAM"]) / (1024 * 1024);
                if (queryObj["DriverVersion"] is not null)
                    gpuInfo.DriverVersion = queryObj["DriverVersion"].ToString() ?? string.Empty;
                gpus.Add(gpuInfo);
            }
        }
        catch
        {
        }
        return gpus;
    }

    private long GetWindowsSystemMemorySizeMb()
    {
        try
        {
            using var searcher = new System.Management.ManagementObjectSearcher(@"root\CIMV2", "SELECT TotalPhysicalMemory FROM Win32_ComputerSystem");
            foreach (System.Management.ManagementObject queryObj in searcher.Get())
            {
                if (queryObj["TotalPhysicalMemory"] is not null and not DBNull)
                {
                    return Convert.ToInt64(queryObj["TotalPhysicalMemory"]) / (1024 * 1024);
                }
            }
        }
        catch
        {
        }
        return 0;
    }

    private string GetLinuxCpuName()
    {
        try
        {
            if (System.IO.File.Exists("/proc/cpuinfo"))
            {
                var lines = System.IO.File.ReadAllLines("/proc/cpuinfo");
                foreach (var line in lines)
                {
                    if (line.StartsWith("model name"))
                    {
                        return line.Split(':')[1].Trim();
                    }
                }
            }
        }
        catch
        {
        }
        return "Unknown CPU";
    }

    private List<GPUInfo> GetLinuxGpuInfo()
    {
        var gpus = new List<GPUInfo>();
        try
        {
            if (System.IO.File.Exists("/proc/driver/nvidia/gpus"))
            {
                var gpuDirs = System.IO.Directory.GetDirectories("/proc/driver/nvidia/gpus");
                foreach (var dir in gpuDirs)
                {
                    var infoFile = System.IO.Path.Combine(dir, "information");
                    if (System.IO.File.Exists(infoFile))
                    {
                        var lines = System.IO.File.ReadAllLines(infoFile);
                        var gpuInfo = new GPUInfo();
                        foreach (var line in lines)
                        {
                            if (line.StartsWith("Model:"))
                                gpuInfo.Name = line.Split(':')[1].Trim();
                        }
                        gpus.Add(gpuInfo);
                    }
                }
            }
        }
        catch
        {
        }
        return gpus;
    }

    private long GetLinuxSystemMemorySizeMb()
    {
        try
        {
            if (System.IO.File.Exists("/proc/meminfo"))
            {
                var lines = System.IO.File.ReadAllLines("/proc/meminfo");
                foreach (var line in lines)
                {
                    if (line.StartsWith("MemTotal:"))
                    {
                        var parts = line.Split(':');
                        if (parts.Length > 1)
                        {
                            var value = parts[1].Trim().Split(' ')[0];
                            if (long.TryParse(value, out var kb))
                            {
                                return kb / 1024;
                            }
                        }
                    }
                }
            }
        }
        catch
        {
        }
        return 0;
    }

    private string GetMacCpuName()
    {
        try
        {
            using var process = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "sysctl",
                    Arguments = "-n machdep.cpu.brand_string",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();
            var output = process.StandardOutput.ReadToEnd().Trim();
            process.WaitForExit();
            return output;
        }
        catch
        {
        }
        return "Unknown CPU";
    }

    private List<GPUInfo> GetMacGpuInfo()
    {
        var gpus = new List<GPUInfo>();
        try
        {
            using var process = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "system_profiler",
                    Arguments = "SPDisplaysDataType",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();
            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            var lines = output.Split('\n');
            var gpuInfo = new GPUInfo();
            foreach (var line in lines)
            {
                if (line.TrimStart().StartsWith("Chipset Model:"))
                {
                    if (gpuInfo.Name != string.Empty)
                        gpus.Add(gpuInfo);
                    gpuInfo = new GPUInfo();
                    gpuInfo.Name = line.Split(':')[1].Trim();
                }
                else if (line.TrimStart().StartsWith("VRAM (Total):"))
                {
                    var value = line.Split(':')[1].Trim();
                    if (value.EndsWith(" MB"))
                    {
                        if (long.TryParse(value.Replace(" MB", "").Trim(), out var mb))
                        {
                            gpuInfo.Memory = mb;
                        }
                    }
                }
            }
            if (gpuInfo.Name != string.Empty)
                gpus.Add(gpuInfo);
        }
        catch
        {
        }
        return gpus;
    }

    private long GetMacSystemMemorySizeMb()
    {
        try
        {
            using var process = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "sysctl",
                    Arguments = "-n hw.memsize",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();
            var output = process.StandardOutput.ReadToEnd().Trim();
            process.WaitForExit();
            if (long.TryParse(output, out var bytes))
            {
                return bytes / (1024 * 1024);
            }
        }
        catch
        {
        }
        return 0;
    }
}