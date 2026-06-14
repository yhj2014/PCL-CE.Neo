using System.Text.Json.Serialization;

namespace PCL_CE.Neo.Core.Link.McPing;

/// <summary>
/// Minecraft 服务器 Mod 信息条目
/// </summary>
public record McPingModInfoModResult(
    [property: JsonPropertyName("modid")] string ModId,
    [property: JsonPropertyName("version")] string Version);