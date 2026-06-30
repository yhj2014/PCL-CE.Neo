using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PCL_CE.Neo.Core.Link.Scaffolding.EasyTier;

public class PublicNodeDto
{
    [JsonPropertyName("data")]
    public required PublicNodeData Data { get; init; }
}

public class PublicNodeData
{
    [JsonPropertyName("items")]
    public required List<PublicNodeItem> Items { get; init; }
}

public class PublicNodeItem
{
    [JsonPropertyName("host")]
    public required string Host { get; init; }

    [JsonPropertyName("is_active")]
    public bool IsActive { get; init; }

    [JsonPropertyName("is_allow_relay")]
    public bool IsAllowRelay { get; init; }
}