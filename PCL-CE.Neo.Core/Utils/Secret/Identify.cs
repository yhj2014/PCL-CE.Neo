using System;
using System.Text;
using PCL_CE.Neo.Core.Logging;
using PCL_CE.Neo.Core.Utils.Exts;
using PCL_CE.Neo.Core.Utils.Hash;

namespace PCL_CE.Neo.Core.Utils.Secret;

public class Identify
{
    public static byte[] RawId { get => _rawId ??= _GetRawId(); }
    private static byte[]? _rawId;

    public static string LauncherId { get => _launcherId ??= _GetLauncherId(); }
    private static string? _launcherId;

    private static byte[] _GetRawId()
    {
        var code = new StringBuilder();
        try
        {
            code.Append("UUID:").Append(_GetSystemProperty("UUID"))
                .Append("|MB_Prod:").Append(_GetSystemProperty("MotherboardProduct"))
                .Append("|MB_SN:").Append(_GetSystemProperty("MotherboardSerialNumber"))
                .Append("|CPU:").Append(_GetSystemProperty("ProcessorId"));
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, "Identify", "Failed to get device basic information");
        }

        return Encoding.UTF8.GetBytes(SHA512Provider.Instance.ComputeHash(code.ToString()).ToHexString());
    }

    private static string _GetSystemProperty(string propertyName)
    {
        try
        {
            if (PlatformDetector.IsWindows)
            {
                return _GetWmiProperty(propertyName switch
                {
                    "UUID" => ("Win32_ComputerSystemProduct", "UUID"),
                    "MotherboardProduct" => ("Win32_BaseBoard", "Product"),
                    "MotherboardSerialNumber" => ("Win32_BaseBoard", "SerialNumber"),
                    "ProcessorId" => ("Win32_Processor", "ProcessorId"),
                    _ => throw new ArgumentException($"Unknown property: {propertyName}")
                });
            }
            else if (PlatformDetector.IsLinux)
            {
                return _GetLinuxProperty(propertyName);
            }
            else if (PlatformDetector.IsMacOS)
            {
                return _GetMacOSProperty(propertyName);
            }
        }
        catch { }
        return string.Empty;
    }

    private static string _GetWmiProperty((string ClassName, string PropertyName) property)
    {
        try
        {
            using var searcher = new System.Management.ManagementObjectSearcher(
                $"SELECT {property.PropertyName} FROM {property.ClassName}");
            using var results = searcher.Get();
            foreach (var obj in results)
            {
                if (obj[property.PropertyName] is not null)
                    return (obj[property.PropertyName].ToString() ?? string.Empty).Trim();
            }
        }
        catch { }
        return string.Empty;
    }

    private static string _GetLinuxProperty(string propertyName)
    {
        try
        {
            var path = propertyName switch
            {
                "UUID" => "/sys/class/dmi/id/product_uuid",
                "MotherboardProduct" => "/sys/class/dmi/id/board_name",
                "MotherboardSerialNumber" => "/sys/class/dmi/id/board_serial",
                "ProcessorId" => "/proc/cpuinfo",
                _ => string.Empty
            };

            if (!File.Exists(path)) return string.Empty;

            var content = File.ReadAllText(path).Trim();
            if (propertyName == "ProcessorId")
            {
                foreach (var line in content.Split('\n'))
                {
                    if (line.StartsWith("processor"))
                        return line.Split(':')[1].Trim();
                }
                return string.Empty;
            }
            return content;
        }
        catch { }
        return string.Empty;
    }

    private static string _GetMacOSProperty(string propertyName)
    {
        try
        {
            var cmd = propertyName switch
            {
                "UUID" => "system_profiler SPHardwareDataType | grep 'Hardware UUID'",
                "MotherboardProduct" => "system_profiler SPHardwareDataType | grep 'Model Name'",
                "MotherboardSerialNumber" => "system_profiler SPHardwareDataType | grep 'Serial Number'",
                "ProcessorId" => "sysctl -n machdep.cpu.brand_string",
                _ => string.Empty
            };

            using var process = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"{cmd}\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();
            var output = process.StandardOutput.ReadToEnd().Trim();
            process.WaitForExit();
            return output.Split(':')[^1].Trim();
        }
        catch { }
        return string.Empty;
    }

    private static string _GetLauncherId()
    {
        try
        {
            var prefix = "PCL-CE|"u8.ToArray();
            var ctx = RawId;
            var suffix = "|LauncherId"u8.ToArray();

            var buffer = new byte[prefix.Length + ctx.Length + suffix.Length];
            var bufferSpan = buffer.AsSpan();
            prefix.CopyTo(bufferSpan[..prefix.Length]);
            ctx.CopyTo(bufferSpan.Slice(prefix.Length, ctx.Length));
            suffix.CopyTo(bufferSpan.Slice(prefix.Length + ctx.Length, suffix.Length));

            Array.Clear(ctx);
            var sample = SHA512Provider.Instance.ComputeHash(bufferSpan).ToHexString();
            bufferSpan.Clear();

            return sample.Substring(64, 16)
                .ToUpper()
                .Insert(4, "-")
                .Insert(9, "-")
                .Insert(14, "-");
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, "Identify", "Cannot get launcher ID");
            return "PCL2-CECE-GOOD-2025";
        }
    }
}