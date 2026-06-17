using System.Text.Json.Serialization;

namespace PCL_CE.Neo.Core.Minecraft.Yggdrasil;

public record Agent
{
    [JsonPropertyName("name")] public string Name { get; init; } = "minecraft";
    [JsonPropertyName("version")] public int Version { get; init; } = 1;
}