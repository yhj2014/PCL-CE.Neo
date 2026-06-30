using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PCL_CE.Neo.Core.Link.Scaffolding.Client.Models;
using PCL_CE.Neo.Core.Link.Scaffolding.EasyTier;
using PCL_CE.Neo.Core.Link.Scaffolding.Exceptions;
using PCL_CE.Neo.Core.Link.Scaffolding.Server;
using PCL_CE.Neo.Core.Utils;

namespace PCL_CE.Neo.Core.Link.Scaffolding;

public static class ScaffoldingFactory
{
    private const string HostIp = "10.114.51.41";

    public static async Task<ScaffoldingClientEntity> CreateClientAsync(
        string playerName, string lobbyCode, LobbyType from, ILoggerFactory loggerFactory)
    {
        var machineId = Guid.NewGuid().ToString();

        if (!LobbyCodeGenerator.TryParse(lobbyCode, out var info))
        {
            throw new ArgumentException("无效的大厅代码", nameof(lobbyCode));
        }

        var logger = loggerFactory.CreateLogger<EasyTierEntity>();
        var etEntity = _CreateEasyTierEntity(info, 0, 0, false, logger);
        etEntity.Launch();

        var (etStatus, players) = await etEntity.CheckEasyTierStatusAsync().ConfigureAwait(false);

        if (!etStatus || players is null)
        {
            throw new InvalidOperationException("获取 EasyTier 信息失败");
        }

        if (players.Players is null)
        {
            throw new FailedToGetPlayerException();
        }

        var hostInfo = players.Host;

        if (hostInfo is null)
        {
            await etEntity.StopAsync().ConfigureAwait(false);
            throw new FailedToGetPlayerException("无法获取主机信息");
        }

        if (!int.TryParse(hostInfo.HostName[22..], out var scfPort))
        {
            await etEntity.StopAsync().ConfigureAwait(false);
            throw new ArgumentException("无效的主机名", nameof(hostInfo));
        }

        var localPort = await etEntity.AddPortForwardAsync(hostInfo.Ip, scfPort).ConfigureAwait(false);

        var clientLogger = loggerFactory.CreateLogger<ScaffoldingClient>();
        var client = new ScaffoldingClient("127.0.0.1", localPort, playerName, machineId, clientLogger);

        return new ScaffoldingClientEntity(client, etEntity, hostInfo);
    }

    public static ScaffoldingServerEntity CreateServer(int mcPort, string playerName, ILoggerFactory loggerFactory)
    {
        var contextLogger = loggerFactory.CreateLogger<ScaffoldingServerContext>();
        var context = ScaffoldingServerContext.Create(playerName, mcPort, contextLogger);
        var scfPort = NetUtils.GetAvailablePort();

        var logger = loggerFactory.CreateLogger<EasyTierEntity>();
        var etEntity = _CreateEasyTierEntity(context.UserLobbyInfo, mcPort, scfPort, true, logger);
        var res = etEntity.Launch();
        if (res != 0)
        {
            throw new InvalidOperationException("启动 EasyTier Core 失败");
        }

        var serverLogger = loggerFactory.CreateLogger<ScaffoldingServer>();
        var server = new ScaffoldingServer(scfPort, context, serverLogger);

        return new ScaffoldingServerEntity(server, etEntity);
    }

    private static EasyTierEntity _CreateEasyTierEntity(LobbyInfo lobby, int mcPort, int port, bool asHost, ILogger<EasyTierEntity> logger) =>
        new(lobby, mcPort, port, asHost, logger);
}

public record ScaffoldingClientEntity(ScaffoldingClient Client, EasyTierEntity EasyTier, EasyPlayerInfo HostInfo);

public record ScaffoldingServerEntity(ScaffoldingServer Server, EasyTierEntity EasyTier);