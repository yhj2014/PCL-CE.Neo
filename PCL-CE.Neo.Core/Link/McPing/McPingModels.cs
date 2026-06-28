using System.Text.Json.Serialization;

namespace PCL_CE.Neo.Core.Link.McPing;

public record McPingVersionResult(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("protocol")] int Protocol);

public record McPingPlayerResult(
    [property: JsonPropertyName("max")] int Max,
    [property: JsonPropertyName("online")] int Online,
    [property: JsonPropertyName("sample")] List<McPingPlayerSampleResult>? Samples);

public record McPingPlayerSampleResult(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("id")] string Id);

public record McPingModInfoResult(
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("modList")] List<McPingModInfoModResult> ModList);

public record McPingModInfoModResult(
    [property: JsonPropertyName("modid")] string Id,
    [property: JsonPropertyName("version")] string Version);

public record McPingResult(
    [property: JsonPropertyName("version")] McPingVersionResult Version,
    [property: JsonPropertyName("players")] McPingPlayerResult Players,
    [property: JsonPropertyName("description")] string Description,
    [property: JsonPropertyName("favicon")] string? Favicon,
    [property: JsonPropertyName("latency")] long Latency,
    [property: JsonPropertyName("modinfo")] McPingModInfoResult? ModInfo,
    [property: JsonPropertyName("preventsChatReports")] bool? PreventsChatReports);
