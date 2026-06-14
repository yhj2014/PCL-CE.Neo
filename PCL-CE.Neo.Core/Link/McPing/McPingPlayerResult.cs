using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PCL_CE.Neo.Core.Link.McPing;

/// <summary>
/// Minecraft 服务器玩家信息
/// </summary>
public record McPingPlayerResult(
    [property: JsonPropertyName("max")] int Max,
    [property: JsonPropertyName("online")] int Online,
    [property: JsonPropertyName("sample")] List<McPingPlayerSampleResult>? Samples);