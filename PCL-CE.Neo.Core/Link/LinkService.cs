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

public interface ILinkService
{
    Task<ServerInfo?> PingServerAsync(string address, int port);
    Task<string> GetLobbyCodeAsync();
    Task<bool> JoinLobbyAsync(string code);
}

public class LinkService : ILinkService
{
    private readonly ILogger<LinkService> _logger;
    private readonly INetworkService _networkService;

    public LinkService(ILogger<LinkService> logger, INetworkService networkService)
    {
        _logger = logger;
        _networkService = networkService;
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
        return Task.FromResult(code);
    }

    public Task<bool> JoinLobbyAsync(string code)
    {
        return Task.FromResult(true);
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
