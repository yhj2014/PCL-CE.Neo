using System.Text.Json.Serialization;

namespace PCL_CE.Neo.Core.Minecraft.Yggdrasil;

public record Profile
{
    [JsonPropertyName("id")] public required string Id { get; init; }
    [JsonPropertyName("name")] public string? Name { get; init; }
    [JsonPropertyName("properties")] public PlayerProperty[]? Properties { get; init; }
}

public record PlayerProperty
{
    [JsonPropertyName("name")] public required string Name { get; init; }
    [JsonPropertyName("value")] public required string Value { get; init; }
    [JsonPropertyName("signature")] public string? Signature { get; init; }
}

public record PlayerTextureProperty
{
    [JsonPropertyName("timestamp")] public required long Timestamp { get; init; }
    [JsonPropertyName("profileId")] public required string ProfileId { get; init; }
    [JsonPropertyName("profileName")] public required string ProfileName { get; init; }
    [JsonPropertyName("textures")] public required PlayerTextures Textures { get; init; }
}

public record PlayerTextures
{
    [JsonPropertyName("skin")] public PlayerTexture? Skin { get; init; }
    [JsonPropertyName("cape")] public PlayerTexture? Cape { get; init; }
}

public record PlayerTexture
{
    [JsonPropertyName("url")] public required string Url { get; init; }
    [JsonPropertyName("metadata")] public PlayerTextureMetadata? Metadata { get; init; }
}

public record PlayerTextureMetadata
{
    [JsonPropertyName("model")] public string? Model { get; init; }
}