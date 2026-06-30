using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PCL_CE.Neo.Core.Link.EasyTier;
using PCL_CE.Neo.Core.Link.Scaffolding.Client.Models;
using PCL_CE.Neo.Core.Utils;

namespace PCL_CE.Neo.Core.Link.Scaffolding.EasyTier;

public enum EtState
{
    Stopped,
    Active,
    Ready
}

public class EasyTierEntity
{
    private readonly Process _etProcess;
    private readonly int _rpcPort;
    private readonly LobbyInfo _lobby;
    private readonly int _scfPort;
    private readonly ILogger<EasyTierEntity> _logger;

    public int ForwardPort { get; private set; }
    public int MinecraftPort { get; init; }
    public EtState State { get; private set; }
    public LobbyInfo Lobby => _lobby;

    public event Action? EasyTierProcessExisted;

    public EasyTierEntity(LobbyInfo lobby, int minecraftPort, int scfPort, bool asHost, ILogger<EasyTierEntity> logger)
    {
        _logger = logger;
        _lobby = lobby;
        MinecraftPort = minecraftPort;
        _scfPort = scfPort;
        State = EtState.Stopped;

        var existEntities = Process.GetProcessesByName("easytier-core");
        foreach (var entity in existEntities)
        {
            _logger.LogWarning("发现已存在的 EasyTier 实例，可能会影响某些功能: {ProcessId}", entity.Id);
        }

        _logger.LogInformation("EasyTier 文件夹路径: {EasyTierFilePath}", EasyTierMetadata.EasyTierFilePath);

        if (!(File.Exists(Path.Combine(EasyTierMetadata.EasyTierFilePath, "easytier-core.exe")) &&
              File.Exists(Path.Combine(EasyTierMetadata.EasyTierFilePath, "easytier-cli.exe")) &&
              File.Exists(Path.Combine(EasyTierMetadata.EasyTierFilePath, "Packet.dll"))))
        {
            _logger.LogError("EasyTier 不存在或不完整");
            throw new FileNotFoundException("EasyTier 不存在或不完整");
        }

        State = EtState.Ready;

        _rpcPort = NetUtils.GetAvailablePort();
        ForwardPort = NetUtils.GetAvailablePort();

        _etProcess = _BuildProcessAsync(asHost).GetAwaiter().GetResult();
    }

    public int Launch()
    {
        _logger.LogInformation("启动 EasyTier Core");

        try
        {
            _etProcess.Start();
            State = EtState.Active;

            _etProcess.Exited += (_, _) => EasyTierProcessExisted?.Invoke();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "启动 EasyTier 失败");
            State = EtState.Stopped;
            return 1;
        }

        return 0;
    }

    public Task<int> StopAsync()
    {
        return Task.Run(() =>
        {
            try
            {
                if (!_etProcess.HasExited)
                {
                    _etProcess.Kill(true);
                    _etProcess.WaitForExit(5000);
                }

                State = EtState.Stopped;
                return 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "停止 EasyTier 失败");
                State = EtState.Stopped;
                return 1;
            }
        });
    }

    private async Task<Process> _BuildProcessAsync(bool asHost)
    {
        var process = new Process
        {
            EnableRaisingEvents = true,
            StartInfo = new ProcessStartInfo
            {
                FileName = Path.Combine(EasyTierMetadata.EasyTierFilePath, "easytier-core.exe"),
                WorkingDirectory = EasyTierMetadata.EasyTierFilePath,
                WindowStyle = ProcessWindowStyle.Hidden
            }
        };

        var args = new ArgumentsBuilder();

        args.AddFlag("no-tun")
            .AddFlag("multi-thread")
            .AddFlag("enable-kcp-proxy")
            .AddFlag("enable-quic-proxy")
            .Add("encryption-algorithm", "aes-gcm")
            .Add("compression", "zstd")
            .Add("default-protocol", "quic")
            .Add("network-name", _lobby.NetworkName)
            .Add("network-secret", _lobby.NetworkSecret)
            .Add("machine-id", Guid.NewGuid().ToString())
            .Add("rpc-portal", _rpcPort.ToString())
            .Add("private-mode", "true")
            .AddFlag("p2p-only");

        if (asHost)
        {
            args.AddWithSpace("i", "10.114.51.41")
                .Add("hostname", $"scaffolding-mc-server-{_scfPort}")
                .Add("tcp-whitelist", _scfPort.ToString())
                .Add("udp-whitelist", _scfPort.ToString())
                .Add("tcp-whitelist", MinecraftPort.ToString())
                .Add("udp-whitelist", MinecraftPort.ToString())
                .Add("l", "tcp://0.0.0.0:0")
                .Add("l", "udp://0.0.0.0:0");
        }
        else
        {
            args.AddFlag("d")
                .Add("hostname", Guid.NewGuid().ToString())
                .Add("tcp-whitelist", "0")
                .Add("udp-whitelist", "0")
                .Add("l", "tcp://0.0.0.0:0")
                .Add("l", "udp://0.0.0.0:0");
        }

        foreach (var address in _fallbackNodeLinks)
        {
            args.Add("p", address);
        }

        process.StartInfo.Arguments = args.GetResult();
        _logger.LogDebug("EasyTier 启动参数: {Arguments}", process.StartInfo.Arguments);

        return process;
    }

    private readonly string[] _fallbackNodeLinks =
    [
        "tcp://public.easytier.top:11010",
        "tcp://public2.easytier.cn:54321",
        "https://etnode.zkitefly.eu.org/node1",
        "https://etnode.zkitefly.eu.org/node2",
        "https://etnode.zkitefly.eu.org/-node1",
        "https://etnode.zkitefly.eu.org/-node2"
    ];

    public async Task<(bool, EtPlayerList?)> CheckEasyTierStatusAsync()
    {
        var retryCount = 0;

        while (_etProcess is null && retryCount < 10)
        {
            await Task.Delay(1000).ConfigureAwait(false);
            retryCount++;
        }

        if (_etProcess is null)
        {
            return (false, null);
        }

        retryCount = 0;
        while (State is not EtState.Ready && retryCount < 10)
        {
            var info = await _GetPlayersAsync().ConfigureAwait(false);
            if (info.Host is null)
            {
                _logger.LogDebug("重试获取 EasyTier 信息");
                await Task.Delay(1000).ConfigureAwait(false);
                retryCount++;
                continue;
            }

            _logger.LogDebug("成功从 EasyTier CLI 获取玩家信息");

            if (info.Host.Ping < 1000)
            {
                State = EtState.Ready;
                return (true, info);
            }

            await Task.Delay(1000).ConfigureAwait(false);
            retryCount++;
        }

        _logger.LogDebug("无法从 EasyTier CLI 获取玩家信息");
        return (false, null);
    }

    public async Task<int> AddPortForwardAsync(string targetIp, int targetPort)
    {
        var localPort = NetUtils.GetAvailablePort();
        using var cliProcess = new Process();
        cliProcess.StartInfo = new ProcessStartInfo
        {
            FileName = Path.Combine(EasyTierMetadata.EasyTierFilePath, "easytier-cli.exe"),
            WorkingDirectory = EasyTierMetadata.EasyTierFilePath,
            ErrorDialog = false,
            CreateNoWindow = true,
            WindowStyle = ProcessWindowStyle.Hidden,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            RedirectStandardInput = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8,
            StandardInputEncoding = Encoding.UTF8
        };
        cliProcess.EnableRaisingEvents = true;

        try
        {
            cliProcess.StartInfo.Arguments =
                $"--rpc-portal 127.0.0.1:{_rpcPort} port-forward add tcp 127.0.0.1:{localPort} {targetIp}:{targetPort}";
            cliProcess.Start();
            await cliProcess.WaitForExitAsync().ConfigureAwait(false);

            cliProcess.StartInfo.Arguments =
                $"--rpc-portal 127.0.0.1:{_rpcPort} port-forward add udp 127.0.0.1:{localPort} {targetIp}:{targetPort}";
            cliProcess.Start();
            await cliProcess.WaitForExitAsync().ConfigureAwait(false);

            cliProcess.StartInfo.Arguments =
                $"--rpc-portal 127.0.0.1:{_rpcPort} port-forward add tcp [::]:{localPort} {targetIp}:{targetPort}";
            cliProcess.Start();
            await cliProcess.WaitForExitAsync().ConfigureAwait(false);

            cliProcess.StartInfo.Arguments =
                $"--rpc-portal 127.0.0.1:{_rpcPort} port-forward add udp [::]:{localPort} {targetIp}:{targetPort}";
            cliProcess.Start();
            await cliProcess.WaitForExitAsync().ConfigureAwait(false);

            _logger.LogDebug("ET Cli 输出: {Output}",
                await cliProcess.StandardOutput.ReadToEndAsync().ConfigureAwait(false) +
                await cliProcess.StandardError.ReadToEndAsync().ConfigureAwait(false));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "添加端口转发失败");
        }

        return localPort;
    }

    private async Task<EtPlayerList> _GetPlayersAsync()
    {
        using var cliProcess = new Process();
        cliProcess.StartInfo = new ProcessStartInfo
        {
            FileName = Path.Combine(EasyTierMetadata.EasyTierFilePath, "easytier-cli.exe"),
            WorkingDirectory = EasyTierMetadata.EasyTierFilePath,
            Arguments = $"--rpc-portal 127.0.0.1:{_rpcPort} -o json peer",
            ErrorDialog = false,
            CreateNoWindow = true,
            WindowStyle = ProcessWindowStyle.Hidden,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            RedirectStandardInput = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8,
            StandardInputEncoding = Encoding.UTF8
        };
        cliProcess.EnableRaisingEvents = true;

        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(5000));

        try
        {
            _logger.LogDebug("尝试获取玩家信息");

            cliProcess.Start();
            cliProcess.StandardInput.Close();

            var stdOut = await cliProcess.StandardOutput.ReadToEndAsync(cts.Token).ConfigureAwait(false);
            var stdErr = await cliProcess.StandardError.ReadToEndAsync(cts.Token).ConfigureAwait(false);

            await cliProcess.WaitForExitAsync(cts.Token).ConfigureAwait(false);

            var output = stdOut + stdErr;

            if (JsonNode.Parse(output) is not JsonArray jArray)
            {
                return new EtPlayerList(null, null);
            }

            List<EasyPlayerInfo> players = [];
            EasyPlayerInfo? host = null;

            foreach (var arr in jArray)
            {
                var info = arr.Deserialize<ETPeerInfo>();
                if (info == null)
                {
                    continue;
                }

                if (info.Hostname.StartsWith("scaffolding-mc-server-", StringComparison.Ordinal))
                {
                    _logger.LogDebug("发现主机玩家: {Hostname}", info.Hostname);

                    if (host is not null)
                    {
                        throw new ArgumentException("主机重复", nameof(host));
                    }

                    host = _ConvertPeerToPlayer(info);
                    continue;
                }

                _logger.LogDebug("发现玩家: {Hostname}", info.Hostname);
                players.Add(_ConvertPeerToPlayer(info));
            }

            var result = host is null ? players : [host, .. players];
            return new EtPlayerList(result, host);
        }
        catch (TaskCanceledException tce)
        {
            _logger.LogError(tce, "读取 CLI 输出超时");
            return new EtPlayerList(null, null);
        }
        catch (ArgumentException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取 EasyTier 玩家列表失败");
            return new EtPlayerList(null, null);
        }
    }

    private static EasyPlayerInfo _ConvertPeerToPlayer(ETPeerInfo info)
    {
        return new EasyPlayerInfo
        {
            IsHost = info.Hostname.StartsWith("scaffolding-mc-server", StringComparison.Ordinal),
            HostName = info.Hostname,
            Ip = info.Ipv4 ?? string.Empty,
            Ping = Math.Round(Convert.ToDouble(info.Ping != "-" ? info.Ping : "0")),
            Loss = Math.Round(Convert.ToDouble(info.Loss != "-" ? info.Loss.Replace("%", "") : "0")),
            NatType = info.NatType,
            EasyTierVer = info.ETVersion
        };
    }
}