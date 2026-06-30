using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using PCL_CE.Neo.Core.Link.Scaffolding.Client.Models;

namespace PCL_CE.Neo.Core.Link.Scaffolding.Server.Abstractions;

public interface IServerContext
{
    IReadOnlyList<PlayerProfile> PlayerProfiles { get; }
    ConcurrentDictionary<string, TrackedPlayerProfile> TrackedPlayers { get; }
    void OnPlayerProfilesChanged();
    event Action<IReadOnlyList<PlayerProfile>> PlayerProfilesPing;
    int MinecraftServerProt { get; }
    LobbyInfo UserLobbyInfo { get; }
    string PlayerName { get; }
}