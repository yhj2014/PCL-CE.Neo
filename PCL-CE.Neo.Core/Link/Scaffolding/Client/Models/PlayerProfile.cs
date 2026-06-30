namespace PCL_CE.Neo.Core.Link.Scaffolding.Client.Models;

public record PlayerProfile(
    string MachineId,
    string PlayerName,
    PlayerKind Kind
);