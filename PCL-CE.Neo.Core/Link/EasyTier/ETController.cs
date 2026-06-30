using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using PCL_CE.Neo.Core.App;
using PCL_CE.Neo.Core.Utils;

namespace PCL_CE.Neo.Core.Link.EasyTier;

public enum ETState
{
    Stopped,
    Running,
    Ready
}

public static class ETController
{
    public static Process? ETProcess { get; private set; }
    public static int ETRpcPort { get; private set; }
    public static ETState Status { get; internal set; }

    private static readonly ILogger _logger = ServiceLocator.GetService<ILogger<ETController>>() ?? new Microsoft.Extensions.Logging.Abstractions.NullLogger<ETController>();

    public static int Precheck()
    {
        var existedET = Process.GetProcessesByName("easytier-core");
        foreach (var p in existedET)
        {
            _logger.LogWarning("发现已有的 EasyTier 实例，可能影响与启动器所用的实例通信: {ProcessId}", p.Id);
        }

        _logger.LogInformation("EasyTier 路径: {ETPath}", ETInfoProvider.ETPath);
        var etPath = ETInfoProvider.ETPath;
        if (!(File.Exists(Path.Combine(etPath, "easytier-core.exe")) && 
              File.Exists(Path.Combine(etPath, "easytier-cli.exe")) &&
              File.Exists(Path.Combine(etPath, "Packet.dll"))))
        {
            _logger.LogError("EasyTier 不存在或不完整");
            return 1;
        }

        return 0;
    }

    public static int Launch(bool isHost, string? hostname = null)
    {
        try
        {
            if (Precheck() != 0)
            {
                return 1;
            }

            ETProcess = new Process
            {
                EnableRaisingEvents = true,
                StartInfo = new ProcessStartInfo
                {
                    FileName = Path.Combine(ETInfoProvider.ETPath, "easytier-core.exe"),
                    WorkingDirectory = ETInfoProvider.ETPath,
                    WindowStyle = ProcessWindowStyle.Hidden
                }
            };

            var arguments = new ArgumentsBuilder();

            var name = "PCLCELobbyTest";
            var secret = "PCLCEETLOBBY2025Secret";

            arguments.AddFlag("no-tun");
            arguments.Add("network-name", name);
            arguments.Add("network-secret", secret);
            arguments.Add("relay-network-whitelist", name);
            arguments.Add("private-mode", "true");

            if (isHost)
            {
                _logger.LogInformation("本机作为创建者创建大厅，EasyTier 网络名称: {NetworkName}", name);
                arguments.Add("i", "10.114.51.41");
                arguments.Add("tcp-whitelist", "25565");
                arguments.Add("udp-whitelist", "25565");
            }
            else
            {
                _logger.LogInformation("本机作为加入者加入大厅，EasyTier 网络名称: {NetworkName}", name);
                arguments.AddFlag("d");
                arguments.Add("tcp-whitelist", "0");
                arguments.Add("udp-whitelist", "0");

                var localPort = NetUtils.GetAvailablePort();
                _logger.LogInformation("ET 端口转发: 远程 25565 -> 本地 {LocalPort}", localPort);
                arguments.Add("port-forward", $"tcp://127.0.0.1:{localPort}/10.114.51.41:25565");
                arguments.Add("port-forward", $"udp://127.0.0.1:{localPort}/10.114.51.41:25565");
            }

            var relays = ETRelay.RelayList;
            foreach (var relay in relays)
            {
                arguments.Add("p", relay.Url);
            }

            arguments.AddFlag("enable-quic-proxy");
            arguments.AddFlag("enable-kcp-proxy");
            arguments.AddFlag("use-smoltcp");
            arguments.Add("encryption-algorithm", "chacha20");
            arguments.Add("default-protocol", "quic");
            arguments.Add("compression", "zstd");
            arguments.AddFlag("multi-thread");

            var showName = "default";
            arguments.Add("hostname",
                (isHost ? "H|" : "J|") + showName + (!string.IsNullOrWhiteSpace(hostname) ? "|" + hostname : ""));

            ETRpcPort = NetUtils.GetAvailablePort();
            arguments.Add("rpc-portal", $"127.0.0.1:{ETRpcPort}");

            ETProcess.StartInfo.Arguments = arguments.GetResult();
            _logger.LogInformation("启动 EasyTier");
            ETProcess.Start();
            Status = ETState.Running;
            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "尝试启动 EasyTier 时遇到问题");
            Status = ETState.Stopped;
            ETProcess = null;
            return 1;
        }
    }

    public static void Exit()
    {
        if (Status == ETState.Stopped || ETProcess == null) return;
        try
        {
            _logger.LogInformation("关闭 EasyTier (PID: {ProcessId})", ETProcess.Id);
            ETProcess.Kill();
            ETProcess.WaitForExit(200);
        }
        catch (InvalidOperationException)
        {
            _logger.LogWarning("EasyTier 进程不存在，可能已退出");
        }
        catch (NullReferenceException)
        {
            _logger.LogWarning("EasyTier 进程不存在，可能已退出");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "关闭 EasyTier 时遇到问题");
        }
        finally
        {
            Status = ETState.Stopped;
            ETProcess = null;
        }
    }
}