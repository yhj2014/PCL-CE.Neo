using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace PCL_CE.Neo.Core.Link.Lobby;

public interface ILobbyService
{
    Task<string> CreateLobbyAsync(string serverAddress, int port = 25565);
    Task<LobbyInfo?> JoinLobbyAsync(string lobbyCode);
    Task<bool> LeaveLobbyAsync(string lobbyCode);
    Task<bool> RefreshLobbyAsync(string lobbyCode);
}

public class LobbyService : ILobbyService
{
    private readonly ILobbyInfoProvider _infoProvider;
    private readonly string _lobbyServerAddress = "lobby.pcl-ce.net";
    private readonly int _lobbyServerPort = 10086;

    public LobbyService(ILobbyInfoProvider? infoProvider = null)
    {
        _infoProvider = infoProvider ?? new LobbyInfoProvider();
    }

    public async Task<string> CreateLobbyAsync(string serverAddress, int port = 25565)
    {
        try
        {
            var lobbyCode = _infoProvider.GenerateLobbyCode();
            
            var lobbyInfo = new LobbyInfo
            {
                ServerAddress = serverAddress,
                Port = port,
                CreatedAt = DateTime.Now
            };

            _infoProvider.AddLobbyInfo(lobbyCode, lobbyInfo);

            await SendLobbyDataAsync(lobbyCode, lobbyInfo);

            LogWrapper.Info($"Created lobby: {lobbyCode} -> {serverAddress}:{port}");
            return lobbyCode;
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, $"Failed to create lobby for {serverAddress}:{port}");
            throw;
        }
    }

    public async Task<LobbyInfo?> JoinLobbyAsync(string lobbyCode)
    {
        try
        {
            if (!_infoProvider.ValidateLobbyCode(lobbyCode))
                throw new ArgumentException("Invalid lobby code");

            var cachedInfo = _infoProvider.GetLobbyInfo(lobbyCode);
            if (cachedInfo != null)
                return cachedInfo;

            var info = await FetchLobbyInfoAsync(lobbyCode);
            if (info != null)
                _infoProvider.AddLobbyInfo(lobbyCode, info);

            return info;
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, $"Failed to join lobby: {lobbyCode}");
            return null;
        }
    }

    public async Task<bool> LeaveLobbyAsync(string lobbyCode)
    {
        try
        {
            _infoProvider.RemoveLobbyInfo(lobbyCode);

            await SendLeaveRequestAsync(lobbyCode);

            LogWrapper.Info($"Left lobby: {lobbyCode}");
            return true;
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, $"Failed to leave lobby: {lobbyCode}");
            return false;
        }
    }

    public async Task<bool> RefreshLobbyAsync(string lobbyCode)
    {
        try
        {
            var info = await FetchLobbyInfoAsync(lobbyCode);
            if (info != null)
            {
                _infoProvider.AddLobbyInfo(lobbyCode, info);
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, $"Failed to refresh lobby: {lobbyCode}");
            return false;
        }
    }

    private async Task SendLobbyDataAsync(string lobbyCode, LobbyInfo info)
    {
        try
        {
            using var client = new TcpClient();
            await client.ConnectAsync(_lobbyServerAddress, _lobbyServerPort);

            using var stream = client.GetStream();
            var data = $"CREATE {lobbyCode} {info.ServerAddress} {info.Port}";
            var bytes = Encoding.UTF8.GetBytes(data);
            
            await stream.WriteAsync(bytes);
        }
        catch (Exception ex)
        {
            LogWrapper.Warn(ex, "Failed to send lobby data to server, using local cache");
        }
    }

    private async Task<LobbyInfo?> FetchLobbyInfoAsync(string lobbyCode)
    {
        try
        {
            using var client = new TcpClient();
            await client.ConnectAsync(_lobbyServerAddress, _lobbyServerPort);

            using var stream = client.GetStream();
            var request = $"QUERY {lobbyCode}";
            var requestBytes = Encoding.UTF8.GetBytes(request);
            
            await stream.WriteAsync(requestBytes);

            var buffer = new byte[1024];
            var bytesRead = await stream.ReadAsync(buffer);
            var response = Encoding.UTF8.GetString(buffer, 0, bytesRead);

            var parts = response.Split(' ');
            if (parts.Length >= 3 && parts[0] == "OK")
            {
                return new LobbyInfo
                {
                    ServerAddress = parts[1],
                    Port = int.TryParse(parts[2], out var port) ? port : 25565,
                    CreatedAt = DateTime.Now
                };
            }

            return null;
        }
        catch (Exception ex)
        {
            LogWrapper.Warn(ex, "Failed to fetch lobby info from server");
            return null;
        }
    }

    private async Task SendLeaveRequestAsync(string lobbyCode)
    {
        try
        {
            using var client = new TcpClient();
            await client.ConnectAsync(_lobbyServerAddress, _lobbyServerPort);

            using var stream = client.GetStream();
            var data = $"LEAVE {lobbyCode}";
            var bytes = Encoding.UTF8.GetBytes(data);
            
            await stream.WriteAsync(bytes);
        }
        catch (Exception ex)
        {
            LogWrapper.Warn(ex, "Failed to send leave request to server");
        }
    }
}