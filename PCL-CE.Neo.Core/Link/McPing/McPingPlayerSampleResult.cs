using System.Text.Json.Serialization;

namespace PCL_CE.Neo.Core.Link.McPing;

/// <summary>
/// Minecraft 服务器玩家样本
/// </summary>
public record McPingPlayerSampleResult(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("id")] string Id);