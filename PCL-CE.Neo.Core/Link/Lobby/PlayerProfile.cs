namespace PCL_CE.Neo.Core.Link.Lobby;

public record PlayerProfile(
    string MachineId,
    string Name,
    string? SkinUrl = null,
    long? Ping = null);