using PCL_CE.Neo.Core.Link.Scaffolding.Client.Models;
using PCL_CE.Neo.Core.Link.Scaffolding.Server.Abstractions;
using PCL_CE.Neo.Core.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PCL_CE.Neo.Core.Link.Scaffolding.Server;

public class ScaffoldingServerContext : IServerContext
{
    private readonly ConcurrentDictionary<string, TrackedPlayerProfile> _trackedPlayers = [];

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

    public int MinecraftServerPort { get; }

    public LobbyInfo UserLobbyInfo { get; }

    public string PlayerName { get; }

    private ScaffoldingServerContext(
        ConcurrentDictionary<string, TrackedPlayerProfile> profiles,
        int mcPort,
        LobbyInfo info,
        string playerName)
    {
        TrackedPlayers = profiles;
        MinecraftServerPort = mcPort;
        UserLobbyInfo = info;
        PlayerName = playerName;
    }

    public static ScaffoldingServerContext Create(string playerName, int mcPort)
    {
        var machineId = Utils.Secret.Identify.LauncherId;
        var profile = new PlayerProfile
        {
            Name = playerName,
            MachineId = machineId,
            Vendor = $"PCL CE Neo",
            Kind = PlayerKind.HOST
        };

        var tracked = new TrackedPlayerProfile { Profile = profile, LastSeenUtc = DateTime.UtcNow };

        var roomCode = LobbyCodeGenerator.Generate();

        var profiles = new ConcurrentDictionary<string, TrackedPlayerProfile>();
        profiles.TryAdd(machineId, tracked);

        return new ScaffoldingServerContext(profiles, mcPort, roomCode, playerName);
    }
}