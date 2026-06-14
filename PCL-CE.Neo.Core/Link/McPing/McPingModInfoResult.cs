using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PCL_CE.Neo.Core.Link.McPing;

/// <summary>
/// Minecraft 服务器 Mod 信息（Forge/Fabric 等）
/// </summary>
public record McPingModInfoResult(
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("modList")] List<McPingModInfoModResult>? ModList);