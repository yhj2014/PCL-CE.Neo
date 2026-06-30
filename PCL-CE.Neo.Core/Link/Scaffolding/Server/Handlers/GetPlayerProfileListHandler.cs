using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PCL_CE.Neo.Core.Link.Scaffolding.Client.Models;
using PCL_CE.Neo.Core.Link.Scaffolding.Server.Abstractions;

namespace PCL_CE.Neo.Core.Link.Scaffolding.Server.Handlers;

public class GetPlayerProfileListHandler : IRequestHandler
{
    public string RequestType { get; } = "c:player_profiles_list";
    private readonly ILogger<GetPlayerProfileListHandler> _logger;

    private static readonly JsonSerializerOptions _JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public GetPlayerProfileListHandler(ILogger<GetPlayerProfileListHandler> logger)
    {
        _logger = logger;
    }

    public Task<(byte Status, ReadOnlyMemory<byte> Body)> HandleAsync(
        ReadOnlyMemory<byte> requestBody, IServerContext context, string sessionId, CancellationToken ct)
    {
        var allProfiles = context.PlayerProfiles;
        var responseBody = JsonSerializer.SerializeToUtf8Bytes(allProfiles, _JsonOptions);

        _logger.LogDebug("[{SessionId}] 返回玩家列表，共 {Count} 名玩家", sessionId, allProfiles.Count);
        return Task.FromResult(((byte)0, new ReadOnlyMemory<byte>(responseBody)));
    }
}