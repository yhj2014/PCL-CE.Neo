using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PCL_CE.Neo.Core.Link.Scaffolding.Client.Models;
using PCL_CE.Neo.Core.Link.Scaffolding.Server.Abstractions;

namespace PCL_CE.Neo.Core.Link.Scaffolding.Server.Handlers;

public class PlayerPingHandler : IRequestHandler
{
    public string RequestType { get; } = "c:player_ping";
    private readonly ILogger<PlayerPingHandler> _logger;

    private static readonly JsonSerializerOptions _JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public PlayerPingHandler(ILogger<PlayerPingHandler> logger)
    {
        _logger = logger;
    }

    public Task<(byte Status, ReadOnlyMemory<byte> Body)> HandleAsync(
        ReadOnlyMemory<byte> requestBody, IServerContext context, string sessionId, CancellationToken ct)
    {
        try
        {
            var profile = JsonSerializer.Deserialize<PlayerProfile>(requestBody.Span, _JsonOptions);
            if (profile is null || string.IsNullOrEmpty(profile.MachineId))
            {
                _logger.LogWarning("[{SessionId}] 收到的 player_ping 缺少或为空的 machine_id，忽略", sessionId);
                return Task.FromResult(((byte)32, ReadOnlyMemory<byte>.Empty));
            }

            var listChanged = false;
            context.TrackedPlayers.AddOrUpdate(profile.MachineId,
                _ =>
                {
                    _logger.LogInformation("[{SessionId}] 新玩家 '{PlayerName}' 连接，machine_id: '{MachineId}'", 
                        sessionId, profile.PlayerName, profile.MachineId);
                    var newPlayer = new TrackedPlayerProfile { Profile = profile, LastSeenUtc = DateTime.UtcNow };
                    listChanged = true;
                    return newPlayer;
                },
                (_, existingPlayer) =>
                {
                    existingPlayer.Profile = profile;
                    existingPlayer.LastSeenUtc = DateTime.UtcNow;
                    return existingPlayer;
                });

            if (listChanged)
            {
                context.OnPlayerProfilesChanged();
            }

            return Task.FromResult(((byte)0, ReadOnlyMemory<byte>.Empty));
        }
        catch (JsonException ex)
        {
            _logger.LogWarning("[{SessionId}] 反序列化 player_ping JSON 失败。错误: {Message}", sessionId, ex.Message);
            return Task.FromResult(((byte)32, ReadOnlyMemory<byte>.Empty));
        }
    }
}