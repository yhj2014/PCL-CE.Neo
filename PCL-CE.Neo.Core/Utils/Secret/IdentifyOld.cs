using System;
using PCL_CE.Neo.Core.Logging;
using PCL_CE.Neo.Core.Utils.Hash;
using PCL_CE.Neo.Core.Utils.Exts;

namespace PCL_CE.Neo.Core.Utils.Secret;

[Obsolete("Use PCL_CE.Neo.Core.Utils.Secret.Identify instead")]
public static class IdentifyOld
{
    private const string DefaultRawCode = "B09675A9351CBD1FD568056781FE3966DD936CC9B94E51AB5CF67EEB7E74C075";
    private static readonly Lazy<string?> _LazyCpuId = new(_GetCpuId);

    private static readonly Lazy<string> _LazyRawCode =
        new(() => CpuId is null ? DefaultRawCode : SHA256Provider.Instance.ComputeHash(CpuId).ToHexString().ToUpper());

    private static readonly Lazy<string> _LaunchId = new(_GetLaunchId);

    private static readonly Lazy<string> _LazyEncryptKey =
        new(() => SHA512Provider.Instance.ComputeHash(RawCode).ToHexString().Substring(4, 32).ToUpper());

    public static string GetGuid() => Guid.NewGuid().ToString();
    [Obsolete]
    public static string? CpuId => _LazyCpuId.Value;
    [Obsolete]
    public static string RawCode => _LazyRawCode.Value;
    [Obsolete]
    public static string LaunchId => _LaunchId.Value;
    [Obsolete]
    public static string EncryptKey => _LazyEncryptKey.Value;

    private static string? _GetCpuId()
    {
        try
        {
            if (PlatformDetector.IsWindows)
            {
                using var searcher = new System.Management.ManagementObjectSearcher("SELECT ProcessorId FROM Win32_Processor");
                using var collection = searcher.Get();

                foreach (var item in collection)
                {
                    try
                    {
                        return item["ProcessorId"]?.ToString();
                    }
                    catch (System.Management.ManagementException ex)
                    {
                        LogWrapper.Warn("Identify", $"WMI property read failed: {ex.Message}");
                    }
                    finally
                    {
                        item.Dispose();
                    }
                }
            }
            else if (PlatformDetector.IsLinux)
            {
                var content = File.ReadAllText("/proc/cpuinfo");
                foreach (var line in content.Split('\n'))
                {
                    if (line.StartsWith("processor"))
                        return line.Split(':')[1].Trim();
                }
            }
            else if (PlatformDetector.IsMacOS)
            {
                using var process = new System.Diagnostics.Process
                {
                    StartInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "/bin/bash",
                        Arguments = "-c \"sysctl -n machdep.cpu.brand_string\"",
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

            LogWrapper.Warn("Identify", "No valid CPU ID found");
            return null;
        }
        catch (System.Management.ManagementException ex)
        {
            LogWrapper.Error(ex, "Identify", "WMI query failed");
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, "Identify", "Unexpected system exception");
        }

        return null;
    }

    public static string GetMachineId(string randomId)
    {
        return SHA512Provider.Instance.ComputeHash($"{randomId}|{CpuId}").ToHexString().ToUpper();
    }

    private static string _GetLaunchId()
    {
        try
        {
            var hashCode = GetMachineId(Guid.NewGuid().ToString())
                .Substring(6, 16)
                .Insert(4, "-")
                .Insert(9, "-")
                .Insert(14, "-");
            return hashCode;
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, "Identify", "Cannot get short launcher ID");
            return "PCL2-CECE-GOOD-2025";
        }
    }
}