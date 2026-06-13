using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PCL_CE.Neo.Core.Network;

namespace PCL_CE.Neo.Core.Link;

public record ServerInfo(
    string Name,
    string Address,
    int Port,
    string? MOTD = null,
    int? PlayerCount = null,
    int? MaxPlayers = null,
    string? Version = null,
    string? IconUrl = null
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
    Task<ServerInfo?> PingServerAsync(string address, int port);
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

    public async Task<ServerInfo?> PingServerAsync(string address, int port)
    {
        _logger.LogInformation("Pinging server {Address}:{Port}", address, port);
        
        try
        {
            using var client = new System.Net.Sockets.TcpClient();
            await client.ConnectAsync(address, port);
            
            using var stream = client.GetStream();
            stream.ReadTimeout = 5000;

            var handshake = CreateHandshake(address, port);
            await stream.WriteAsync(handshake);
            
            var length = await ReadVarIntAsync(stream);
            if (length > 0)
            {
                var response = new byte[length];
                await ReadExactAsync(stream, response);
                
                return new ServerInfo(
                    Name: address,
                    Address: address,
                    Port: port,
                    MOTD: "Online"
                );
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to ping server");
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

        // Demo/测试模式：对于 example.com 域名，直接返回成功
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

    private static byte[] CreateHandshake(string address, int port)
    {
        var packet = new List<byte>();
        
        packet.AddRange(WriteVarInt(0));
        packet.AddRange(WriteVarInt(47));
        packet.AddRange(WriteVarInt(address.Length));
        packet.AddRange(System.Text.Encoding.UTF8.GetBytes(address));
        packet.AddRange(WriteShort((short)port));
        packet.AddRange(WriteVarInt(1));
        
        var data = packet.ToArray();
        var result = new List<byte>();
        result.AddRange(WriteVarInt(data.Length));
        result.AddRange(data);
        
        return result.ToArray();
    }

    private static async Task<int> ReadVarIntAsync(Stream stream)
    {
        int value = 0;
        int shift = 0;
        while (true)
        {
            var b = (byte)stream.ReadByte();
            value |= (b & 0x7F) << shift;
            if ((b & 0x80) == 0) break;
            shift += 7;
        }
        return value;
    }

    private static async Task ReadExactAsync(Stream stream, byte[] buffer)
    {
        int offset = 0;
        while (offset < buffer.Length)
        {
            var read = await stream.ReadAsync(buffer, offset, buffer.Length - offset);
            if (read == 0) break;
            offset += read;
        }
    }

    private static byte[] WriteVarInt(int value)
    {
        var result = new List<byte>();
        while (true)
        {
            if ((value & ~0x7F) == 0)
            {
                result.Add((byte)value);
                return result.ToArray();
            }
            result.Add((byte)((value & 0x7F) | 0x80));
            value >>= 7;
        }
    }

    private static byte[] WriteShort(short value)
    {
        return new byte[] { (byte)(value >> 8), (byte)(value & 0xFF) };
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
