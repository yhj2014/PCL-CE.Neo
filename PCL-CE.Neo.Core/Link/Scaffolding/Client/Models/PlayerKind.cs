using System.Text.Json.Serialization;

namespace PCL_CE.Neo.Core.Link.Scaffolding.Client.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PlayerKind
{
    HOST,
    GUEST
}