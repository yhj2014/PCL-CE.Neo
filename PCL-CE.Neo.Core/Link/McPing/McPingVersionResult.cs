using System.Text.Json.Serialization;

namespace PCL_CE.Neo.Core.Link.McPing;

/// <summary>
/// Minecraft 服务器版本信息
/// </summary>
public record McPingVersionResult(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("protocol")] int Protocol);