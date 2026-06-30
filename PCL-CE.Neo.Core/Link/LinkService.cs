using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PCL_CE.Neo.Core.Link.McPing;
using PCL_CE.Neo.Core.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PCL_CE.Neo.Core.Link;

public record ServerInfo(
    string Name,
    string Address,
    int Port,
    string? MOTD = null,
    int? PlayerCount = null,
    int? MaxPlayers = null,
    string? Version = null,
    string? IconUrl = null,
    long? Latency = null
);

public record LobbyInfo(
    string Code,
    string HostName,
    int PlayerCount,
    int MaxPlayers,
    long CreatedAt,
    string? Version = null
);

public interface ILinkService
{
    Task<ServerInfo?> PingServerAsync(string address, int port, CancellationToken cancellationToken = default);
    Task<string> GetLobbyCodeAsync();
    Task<bool> JoinLobbyAsync(string code);
    Task<LobbyInfo?> GetLobbyInfoAsync(string code);
    Task<bool> CreateLobbyAsync(string code, string playerName);
}

public class LinkService : ILinkService
{
    private readonly ILogger<LinkService> _logger;
    private readonly INetworkService _networkService;
    private readonly string _lobbyServerUrl;

    public LinkService(ILogger<LinkService> logger, INetworkService networkService)
    {
        _logger = logger;
        _networkService = networkService;
        _lobbyServerUrl = "https://pcl-link.example.com";
    }

    public async Task<ServerInfo?> PingServerAsync(string address, int port, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Pinging server {Address}:{Port}", address, port);
        
        try
        {
            var result = await McPingServiceFactory.PingAsync(address, port, 10000, cancellationToken);
            
            if (result != null)
            {
                return new ServerInfo(
                    Name: address,
                    Address: address,
                    Port: port,
                    MOTD: result.Description,
                    PlayerCount: result.Players?.Online,
                    MaxPlayers: result.Players?.Max,
                    Version: result.Version?.Name,
                    IconUrl: result.Favicon,
                    Latency: result.Latency
                );
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to ping server {Address}:{Port}", address, port);
        }
        
        return null;
    }

    public Task<string> GetLobbyCodeAsync()
    {
        var code = GenerateLobbyCode();
        _logger.LogInformation("Generated lobby code: {Code}", code);
        return Task.FromResult(code);
    }

    public async Task<bool> JoinLobbyAsync(string code)
    {
        _logger.LogInformation("Attempting to join lobby: {Code}", code);

        if (string.IsNullOrEmpty(code) || code.Length != 6)
        {
            _logger.LogWarning("Invalid lobby code format: {Code}", code);
            return false;
        }

        if (_lobbyServerUrl.Contains("example.com", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogInformation("Demo mode: Lobby {Code} joined successfully", code);
            return true;
        }

        try
        {
            var response = await _networkService.GetStringAsync($"{_lobbyServerUrl}/api/lobby/join/{code}");

            if (string.IsNullOrEmpty(response) || response.Contains("error", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Lobby {Code} not found or no longer available", code);
                return false;
            }

            _logger.LogInformation("Successfully joined lobby: {Code}", code);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to join lobby: {Code}", code);
            return false;
        }
    }

    public async Task<LobbyInfo?> GetLobbyInfoAsync(string code)
    {
        if (string.IsNullOrEmpty(code) || code.Length != 6)
        {
            return null;
        }

        try
        {
            var response = await _networkService.GetStringAsync($"{_lobbyServerUrl}/api/lobby/info/{code}");
            
            if (string.IsNullOrEmpty(response))
            {
                return null;
            }

            var info = System.Text.Json.JsonSerializer.Deserialize<LobbyInfo>(response);
            return info;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get lobby info: {Code}", code);
            return null;
        }
    }

    public async Task<bool> CreateLobbyAsync(string code, string playerName)
    {
        _logger.LogInformation("Creating lobby with code: {Code}", code);

        try
        {
            var payload = System.Text.Json.JsonSerializer.Serialize(new
            {
                Code = code,
                HostName = playerName,
                CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            });

            var response = await _networkService.PostStringAsync($"{_lobbyServerUrl}/api/lobby/create", payload);
            
            if (string.IsNullOrEmpty(response))
            {
                return false;
            }

            _logger.LogInformation("Lobby created successfully: {Code}", code);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create lobby: {Code}", code);
            return false;
        }
    }

    private static string GenerateLobbyCode()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        var random = new Random();
        return new string(Enumerable.Range(0, 6).Select(_ => chars[random.Next(chars.Length)]).ToArray());
    }
}

public static class LinkExtensions
{
    public static IServiceCollection AddLinkService(this IServiceCollection services)
    {
        services.AddSingleton<ILinkService, LinkService>();
        return services;
    }
}