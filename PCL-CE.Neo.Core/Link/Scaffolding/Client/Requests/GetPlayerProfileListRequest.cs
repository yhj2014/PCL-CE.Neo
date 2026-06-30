using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using PCL_CE.Neo.Core.Link.Scaffolding.Client.Abstractions;
using PCL_CE.Neo.Core.Link.Scaffolding.Client.Models;

namespace PCL_CE.Neo.Core.Link.Scaffolding.Client.Requests;

public class GetPlayerProfileListRequest : Request<IReadOnlyList<PlayerProfile>>
{
    public override string RequestType { get; } = "c:player_profiles_list";

    private static readonly JsonSerializerOptions _JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public override IReadOnlyList<PlayerProfile> ParseResponseBody(ReadOnlyMemory<byte> body)
    {
        return JsonSerializer.Deserialize<List<PlayerProfile>>(body.Span, _JsonOptions) ?? [];
    }
}