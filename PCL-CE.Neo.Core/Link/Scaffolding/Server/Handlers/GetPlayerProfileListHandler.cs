using PCL_CE.Neo.Core.Link.Scaffolding.Client.Models;
using PCL_CE.Neo.Core.Link.Scaffolding.Server.Abstractions;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace PCL_CE.Neo.Core.Link.Scaffolding.Server.Handlers;

public class GetPlayerProfileListHandler : IRequestHandler
{
    public string RequestType { get; } = "c:player_profiles_list";

    private static readonly JsonSerializerOptions _JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public Task<(byte Status, ReadOnlyMemory<byte> Body)> HandleAsync(ReadOnlyMemory<byte> requestBody,
        IServerContext context, string sessionId, CancellationToken ct)
    {
        var hostProfile = new PlayerProfile
        {
            Name = context.PlayerName,
            MachineId = Utils.Secret.Identify.LauncherId,
            Vendor = $"PCL CE Neo",
            Kind = PlayerKind.HOST
        };

        var allProfiles = context.PlayerProfiles;

        var responseBody = JsonSerializer.SerializeToUtf8Bytes(allProfiles, _JsonOptions);

        return Task.FromResult(((byte)0, new ReadOnlyMemory<byte>(responseBody)));
    }
}