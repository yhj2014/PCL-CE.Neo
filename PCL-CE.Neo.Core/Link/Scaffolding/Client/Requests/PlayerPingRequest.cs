using System;
using System.Buffers;
using System.Text.Json;
using System.Text.Json.Serialization;
using PCL_CE.Neo.Core.Link.Scaffolding.Client.Abstractions;
using PCL_CE.Neo.Core.Link.Scaffolding.Client.Models;

namespace PCL_CE.Neo.Core.Link.Scaffolding.Client.Requests;

public sealed class PlayerPingRequest(string name, string machineId, string vendor) : IRequest<bool>
{
    private readonly PlayerProfile _profile = new()
    {
        Name = name,
        MachineId = machineId,
        Vendor = vendor
    };

    private static readonly JsonSerializerOptions _JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public string RequestType { get; } = "c:player_ping";

    public void WriteRequestBody(IBufferWriter<byte> writer)
    {
        using var jsonWriter = new Utf8JsonWriter(writer);
        JsonSerializer.Serialize(jsonWriter, _profile, _JsonOptions);
    }

    public bool ParseResponseBody(ReadOnlyMemory<byte> responseBody)
    {
        return responseBody.IsEmpty;
    }
}