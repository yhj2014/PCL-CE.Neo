using System.Text.Json.Serialization;

namespace PCL_CE.Neo.Core.Link.McPing.Model;

public record McPingModInfoModResult(
    [property: JsonPropertyName("modid")] string ModId,
    [property: JsonPropertyName("version")] string Version);