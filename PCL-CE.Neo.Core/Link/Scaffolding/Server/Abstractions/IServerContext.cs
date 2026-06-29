using PCL_CE.Neo.Core.Link.Scaffolding.Client.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace PCL_CE.Neo.Core.Link.Scaffolding.Server.Abstractions;

public interface IServerContext
{
    IReadOnlyList<PlayerProfile> PlayerProfiles { get; }

    ConcurrentDictionary<string, TrackedPlayerProfile> TrackedPlayers { get; }

    void OnPlayerProfilesChanged();

    event Action<IReadOnlyList<PlayerProfile>> PlayerProfilesPing;

    int MinecraftServerPort { get; }

    LobbyInfo UserLobbyInfo { get; }

    string PlayerName { get; }
}