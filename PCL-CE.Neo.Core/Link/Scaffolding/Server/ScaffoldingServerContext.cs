using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PCL_CE.Neo.Core.Link.Scaffolding.Client.Models;
using PCL_CE.Neo.Core.Link.Scaffolding.Server.Abstractions;

namespace PCL_CE.Neo.Core.Link.Scaffolding.Server;

public class ScaffoldingServerContext : IServerContext
{
    private readonly ConcurrentDictionary<string, TrackedPlayerProfile> _trackedPlayers = [];
    private readonly ILogger<ScaffoldingServerContext> _logger;

    public ConcurrentDictionary<string, TrackedPlayerProfile> TrackedPlayers
    {
        get => _trackedPlayers;
        private init => _trackedPlayers = value;
    }

    public IReadOnlyList<PlayerProfile> PlayerProfiles =>
        _trackedPlayers.Values.Select(player => player.Profile).ToList().AsReadOnly();

    public event Action<IReadOnlyList<PlayerProfile>>? PlayerProfilesPing;

    public void OnPlayerProfilesChanged()
    {
        var currentProfiles = PlayerProfiles;
        Task.Run(() => PlayerProfilesPing?.Invoke(currentProfiles));
    }

    public int MinecraftServerProt { get; }
    public LobbyInfo UserLobbyInfo { get; }
    public string PlayerName { get; }

    private ScaffoldingServerContext(
        ConcurrentDictionary<string, TrackedPlayerProfile> profiles,
        int mcPort,
        LobbyInfo info,
        string playerName,
        ILogger<ScaffoldingServerContext> logger)
    {
        TrackedPlayers = profiles;
        MinecraftServerProt = mcPort;
        UserLobbyInfo = info;
        PlayerName = playerName;
        _logger = logger;
    }

    public static ScaffoldingServerContext Create(string playerName, int mcPort, ILogger<ScaffoldingServerContext> logger)
    {
        var machineId = Guid.NewGuid().ToString();
        var profile = new PlayerProfile(machineId, playerName, PlayerKind.HOST);

        var tracked = new TrackedPlayerProfile { Profile = profile, LastSeenUtc = DateTime.UtcNow };
        var roomCode = LobbyCodeGenerator.Generate();

        var profiles = new ConcurrentDictionary<string, TrackedPlayerProfile>();
        profiles.TryAdd(machineId, tracked);

        logger.LogInformation("创建大厅服务器上下文，玩家: {PlayerName}, MC端口: {McPort}, 房间代码: {RoomCode}", 
            playerName, mcPort, roomCode.FullCode);

        return new ScaffoldingServerContext(profiles, mcPort, roomCode, playerName, logger);
    }
}