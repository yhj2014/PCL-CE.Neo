using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using PCL_CE.Neo.Core.Link.Scaffolding.Client.Abstractions;
using PCL_CE.Neo.Core.Link.Scaffolding.Client.Models;

namespace PCL_CE.Neo.Core.Link.Scaffolding.Client.Requests;

public sealed class GetPlayerProfileListRequest : IRequest<IReadOnlyList<PlayerProfile>>
{
    private static readonly JsonSerializerOptions _JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public string RequestType { get; } = "c:player_profiles_list";

    public void WriteRequestBody(IBufferWriter<byte> writer)
    {
    }

    public IReadOnlyList<PlayerProfile> ParseResponseBody(ReadOnlyMemory<byte> responseBody)
    {
        var profiles = JsonSerializer.Deserialize<IReadOnlyList<PlayerProfile>>(responseBody.Span, _JsonOptions);
        return profiles ?? ArraySegment<PlayerProfile>.Empty;
    }
}