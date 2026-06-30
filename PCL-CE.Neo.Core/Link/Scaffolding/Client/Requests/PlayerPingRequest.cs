using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using PCL_CE.Neo.Core.Link.Scaffolding.Client.Abstractions;

namespace PCL_CE.Neo.Core.Link.Scaffolding.Client.Requests;

public class PlayerPingRequest : Request<bool>
{
    public override string RequestType { get; } = "c:player_ping";

    private readonly PlayerPingData _data;

    private static readonly JsonSerializerOptions _JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public PlayerPingRequest(string playerName, string machineId, string vendor)
    {
        _data = new PlayerPingData
        {
            PlayerName = playerName,
            MachineId = machineId,
            Vendor = vendor
        };
    }

    public override ReadOnlyMemory<byte> SerializeRequestBody()
    {
        return JsonSerializer.SerializeToUtf8Bytes(_data, _JsonOptions);
    }

    public override bool ParseResponseBody(ReadOnlyMemory<byte> body)
    {
        return body.Length == 0;
    }

    private class PlayerPingData
    {
        [JsonPropertyName("player_name")]
        public required string PlayerName { get; init; }

        [JsonPropertyName("machine_id")]
        public required string MachineId { get; init; }

        [JsonPropertyName("vendor")]
        public required string Vendor { get; init; }
    }
}