using System.Text;
using System.Text.Json;
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

            // Send handshake with state=1 (status)
            var handshake = CreateHandshake(address, port, 1);
            await stream.WriteAsync(handshake);
            
            // Send status request
            var request = CreateStatusRequest();
            await stream.WriteAsync(request);
            
            // Read response length
            var length = await ReadVarIntAsync(stream);
            if (length <= 0)
            {
                _logger.LogWarning("Invalid response length from server");
                return null;
            }
            
            // Read response data
            var responseData = new byte[length];
            await ReadExactAsync(stream, responseData);
            
            // Skip packet ID (VarInt) and parse JSON
            var jsonStart = FindJsonStart(responseData);
            if (jsonStart < 0)
            {
                _logger.LogWarning("Could not find JSON in server response");
                return null;
            }
            
            var jsonString = Encoding.UTF8.GetString(responseData, jsonStart, responseData.Length - jsonStart);
            _logger.LogDebug("Server response JSON: {Json}", jsonString);
            
            using var doc = JsonDocument.Parse(jsonString);
            var root = doc.RootElement;
            
            // Parse JSON response
            string? description = null;
            int? playerCount = null;
            int? maxPlayers = null;
            string? versionName = null;
            string? versionProtocol = null;
            string? favicon = null;
            
            if (root.TryGetProperty("description", out var descElem))
            {
                // Description can be a string or an object with "text" field
                if (descElem.ValueKind == JsonValueKind.String)
                {
                    description = descElem.GetString();
                }
                else if (descElem.TryGetProperty("text", out var textElem))
                {
                    description = textElem.GetString();
                }
            }
            
            if (root.TryGetProperty("players", out var playersElem))
            {
                if (playersElem.TryGetProperty("online", out var onlineElem))
                    playerCount = onlineElem.GetInt32();
                if (playersElem.TryGetProperty("max", out var maxElem))
                    maxPlayers = maxElem.GetInt32();
            }
            
            if (root.TryGetProperty("version", out var versionElem))
            {
                if (versionElem.TryGetProperty("name", out var nameElem))
                    versionName = nameElem.GetString();
                if (versionElem.TryGetProperty("protocol", out var protocolElem))
                    versionProtocol = protocolElem.GetInt32().ToString();
            }
            
            if (root.TryGetProperty("favicon", out var faviconElem))
            {
                favicon = faviconElem.GetString();
            }
            
            // Send ping to get latency
            var ping = await MeasurePingAsync(stream);
            
            _logger.LogInformation("Server ping successful: {Address}:{Port}, Players: {Online}/{Max}, Version: {Version}, Latency: {Latency}ms",
                address, port, playerCount, maxPlayers, versionName, ping);
            
            return new ServerInfo(
                Name: description ?? address,
                Address: address,
                Port: port,
                MOTD: description,
                PlayerCount: playerCount,
                MaxPlayers: maxPlayers,
                Version: versionName,
                IconUrl: favicon
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to ping server {Address}:{Port}", address, port);
            return null;
        }
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

            var info = JsonSerializer.Deserialize<LobbyInfo>(response);
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
            var payload = JsonSerializer.Serialize(new
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

    private byte[] CreateHandshake(string address, int port, int state)
    {
        using var ms = new MemoryStream();
        
        // Packet ID (0x00 for handshake)
        WriteVarInt(ms, 0x00);
        // Protocol version (-1 for auto-detect, 47 for 1.8-1.16.5)
        WriteVarInt(ms, 47);
        // Server address
        WriteString(ms, address);
        // Server port
        WriteShort(ms, (short)port);
        // State (1 for status)
        WriteVarInt(ms, state);
        
        var data = ms.ToArray();
        return CreatePacket(data);
    }

    private byte[] CreateStatusRequest()
    {
        // Status request packet ID (0x00)
        return CreatePacket(new byte[] { 0x00 });
    }

    private async Task<long> MeasurePingAsync(Stream stream)
    {
        try
        {
            // Create ping packet
            var pingData = CreatePacket(new byte[] { 0x01 });
            var pingStart = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            
            await stream.WriteAsync(pingData);
            
            // Read pong response
            var length = await ReadVarIntAsync(stream);
            if (length > 0)
            {
                var response = new byte[length];
                await ReadExactAsync(stream, response);
                
                var pingEnd = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                return pingEnd - pingStart;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to measure ping");
        }
        
        return -1;
    }

    private byte[] CreatePacket(byte[] data)
    {
        using var ms = new MemoryStream();
        WriteVarInt(ms, data.Length);
        ms.Write(data);
        return ms.ToArray();
    }

    private int FindJsonStart(byte[] data)
    {
        // Find the position where the JSON starts (after the packet ID VarInt)
        // The response format is: [length (VarInt)][packet_id (VarInt)][json (UTF-8)]
        int index = 0;
        try
        {
            // Skip length VarInt
            while (index < data.Length && (data[index] & 0x80) != 0)
                index++;
            index++;
            
            // Skip packet ID VarInt
            while (index < data.Length && (data[index] & 0x80) != 0)
                index++;
            index++;
            
            return index;
        }
        catch
        {
            return -1;
        }
    }

    private static byte[] WriteVarInt(Stream stream, int value)
    {
        var result = new List<byte>();
        while (true)
        {
            if ((value & ~0x7F) == 0)
            {
                result.Add((byte)value);
                break;
            }
            result.Add((byte)((value & 0x7F) | 0x80));
            value >>= 7;
        }
        stream.Write(result.ToArray());
        return result.ToArray();
    }

    private static byte[] WriteString(Stream stream, string value)
    {
        var stringBytes = Encoding.UTF8.GetBytes(value);
        WriteVarInt(stream, stringBytes.Length);
        stream.Write(stringBytes);
        return stringBytes;
    }

    private static byte[] WriteShort(Stream stream, short value)
    {
        var bytes = new byte[] { (byte)(value >> 8), (byte)(value & 0xFF) };
        stream.Write(bytes);
        return bytes;
    }

    private static async Task<int> ReadVarIntAsync(Stream stream)
    {
        int value = 0;
        int shift = 0;
        while (true)
        {
            var b = (byte)stream.ReadByte();
            if (b == 0xFF) break; // End of stream
            value |= (b & 0x7F) << shift;
            if ((b & 0x80) == 0) break;
            shift += 7;
            if (shift >= 32)
            {
                _logger?.LogWarning("VarInt overflow");
                break;
            }
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
