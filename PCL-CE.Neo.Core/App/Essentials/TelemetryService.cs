using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace PCL_CE.Neo.Core.App.Essentials;

public sealed class TelemetryService : IDisposable
{
    private readonly ILogger<TelemetryService> _logger;
    private bool _disposed;

    public TelemetryService(ILogger<TelemetryService> logger)
    {
        _logger = logger;
    }

    public async Task StartAsync(bool enabled)
    {
        if (!enabled)
        {
            _logger.LogDebug("遥测服务已禁用");
            return;
        }
        
        _logger.LogInformation("开始初始化遥测服务");
        try
        {
            await _ReportDeviceEnvironment();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "遥测服务初始化失败");
        }
    }

    public void ReportException(Exception ex, string plain, LogLevel level)
    {
        _logger.LogError(ex, "{Message}", plain);
    }

    private async Task _ReportDeviceEnvironment()
    {
        _logger.LogInformation("正在上报设备环境调查数据");
        
        try
        {
            var telemetry = new TelemetryDeviceEnvironment
            {
                Tag = "Telemetry",
                Id = Guid.NewGuid().ToString(),
                Os = Environment.OSVersion.Version.Build,
                Is64Bit = Environment.Is64BitOperatingSystem,
                IsArm64 = RuntimeInformation.OSArchitecture.Equals(Architecture.Arm64),
                Launcher = "PCL-CE.Neo",
                LauncherBranch = "Unknown",
                Memory = (ulong)Environment.WorkingSet,
                Ipv6Status = "Unknown"
            };
            
            _logger.LogInformation("已收集设备环境数据: OS={OS}, Is64Bit={Is64Bit}, Architecture={Architecture}", 
                telemetry.Os, telemetry.Is64Bit, RuntimeInformation.OSArchitecture);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "设备环境调查数据发送失败");
        }
    }

    private class TelemetryDeviceEnvironment
    {
        public required string Tag { get; set; }
        public required string Id { get; set; }
        [JsonPropertyName("OS")] public required int Os { get; set; }
        public required bool Is64Bit { get; set; }
        [JsonPropertyName("IsARM64")] public required bool IsArm64 { get; set; }
        public required string Launcher { get; set; }
        public required string LauncherBranch { get; set; }
        public required ulong Memory { get; set; }
        public required string Ipv6Status { get; set; }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _logger.LogDebug("遥测服务已关闭");
    }
}