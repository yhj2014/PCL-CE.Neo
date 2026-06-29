using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace PCL_CE.Neo.Core.Link.EasyTier;

public interface IETController
{
    Task<bool> StartAsync(string relayServer, int localPort = 25565);
    Task<bool> StopAsync();
    Task<List<ETPeerInfo>> GetOnlinePeersAsync();
    Task<bool> ConnectToPeerAsync(string peerId, int targetPort = 25565);
    Task<bool> DisconnectFromPeerAsync(string peerId);
    Task<ETPeerInfo?> GetPeerInfoAsync(string peerId);
    bool IsRunning { get; }
    string? LocalPeerId { get; }
}

public class ETController : IETController
{
    private TcpClient? _relayClient;
    private bool _isRunning;
    private string? _localPeerId;
    private readonly Dictionary<string, ETPeerInfo> _peers = new();
    private int _localPort;
    private string? _relayServer;
    private int _relayPort = 10080;

    public bool IsRunning => _isRunning;
    public string? LocalPeerId => _localPeerId;

    public async Task<bool> StartAsync(string relayServer, int localPort = 25565)
    {
        try
        {
            _localPort = localPort;
            var parts = relayServer.Split(':');
            _relayServer = parts[0];
            if (parts.Length > 1)
                _relayPort = int.TryParse(parts[1], out var port) ? port : 10080;

            _relayClient = new TcpClient();
            await _relayClient.ConnectAsync(_relayServer, _relayPort);

            _localPeerId = GeneratePeerId();
            
            await SendRegisterRequestAsync();

            _isRunning = true;
            _ = ListenForUpdatesAsync();

            LogWrapper.Info($"EasyTier started with peer ID: {_localPeerId}");
            return true;
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, $"Failed to start EasyTier: {relayServer}");
            return false;
        }
    }

    public async Task<bool> StopAsync()
    {
        try
        {
            if (_relayClient != null)
            {
                await SendUnregisterRequestAsync();
                _relayClient.Close();
                _relayClient = null;
            }

            _isRunning = false;
            _peers.Clear();

            LogWrapper.Info("EasyTier stopped");
            return true;
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, "Failed to stop EasyTier");
            return false;
        }
    }

    public async Task<List<ETPeerInfo>> GetOnlinePeersAsync()
    {
        try
        {
            await SendListRequestAsync();
            await Task.Delay(500);
            return _peers.Values.Where(p => p.IsOnline).ToList();
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, "Failed to get online peers");
            return new List<ETPeerInfo>();
        }
    }

    public async Task<bool> ConnectToPeerAsync(string peerId, int targetPort = 25565)
    {
        try
        {
            if (!_isRunning || _relayClient == null)
                throw new InvalidOperationException("EasyTier is not running");

            var request = $"CONNECT {peerId} {targetPort}";
            await SendRequestAsync(request);

            LogWrapper.Info($"Connecting to peer: {peerId}");
            return true;
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, $"Failed to connect to peer: {peerId}");
            return false;
        }
    }

    public async Task<bool> DisconnectFromPeerAsync(string peerId)
    {
        try
        {
            if (!_isRunning || _relayClient == null)
                throw new InvalidOperationException("EasyTier is not running");

            var request = $"DISCONNECT {peerId}";
            await SendRequestAsync(request);

            LogWrapper.Info($"Disconnected from peer: {peerId}");
            return true;
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, $"Failed to disconnect from peer: {peerId}");
            return false;
        }
    }

    public Task<ETPeerInfo?> GetPeerInfoAsync(string peerId)
    {
        try
        {
            _peers.TryGetValue(peerId, out var info);
            return Task.FromResult(info);
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, $"Failed to get peer info: {peerId}");
            return Task.FromResult<ETPeerInfo?>(null);
        }
    }

    private string GeneratePeerId()
    {
        return Guid.NewGuid().ToString("N").Substring(0, 16);
    }

    private async Task SendRegisterRequestAsync()
    {
        if (_relayClient == null)
            return;

        var request = $"REGISTER {_localPeerId} {_localPort}";
        await SendRequestAsync(request);
    }

    private async Task SendUnregisterRequestAsync()
    {
        if (_relayClient == null)
            return;

        var request = $"UNREGISTER {_localPeerId}";
        await SendRequestAsync(request);
    }

    private async Task SendListRequestAsync()
    {
        if (_relayClient == null)
            return;

        await SendRequestAsync("LIST");
    }

    private async Task SendRequestAsync(string request)
    {
        if (_relayClient == null || !_relayClient.Connected)
            throw new InvalidOperationException("Not connected to relay server");

        var bytes = Encoding.UTF8.GetBytes(request + "\n");
        await _relayClient.GetStream().WriteAsync(bytes);
    }

    private async Task ListenForUpdatesAsync()
    {
        try
        {
            if (_relayClient == null)
                return;

            var buffer = new byte[4096];
            var stream = _relayClient.GetStream();

            while (_isRunning && _relayClient.Connected)
            {
                var bytesRead = await stream.ReadAsync(buffer);
                if (bytesRead == 0)
                    break;

                var response = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                await ProcessResponseAsync(response);
            }
        }
        catch (Exception ex)
        {
            LogWrapper.Warn(ex, "EasyTier listener stopped");
            _isRunning = false;
        }
    }

    private async Task ProcessResponseAsync(string response)
    {
        try
        {
            var lines = response.Split('\n');
            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (string.IsNullOrEmpty(trimmed))
                    continue;

                var parts = trimmed.Split(' ');
                switch (parts[0])
                {
                    case "PEER":
                        if (parts.Length >= 6)
                        {
                            var peer = new ETPeerInfo
                            {
                                PeerId = parts[1],
                                Name = parts[2],
                                IpAddress = parts[3],
                                Port = int.Parse(parts[4]),
                                IsOnline = parts[5] == "ONLINE",
                                LastSeen = DateTime.Now
                            };
                            _peers[peer.PeerId] = peer;
                        }
                        break;
                    case "STATUS":
                        if (parts.Length >= 2)
                        {
                            LogWrapper.Info($"EasyTier status: {parts[1]}");
                        }
                        break;
                    case "ERROR":
                        LogWrapper.Warn($"EasyTier error: {trimmed.Substring(6)}");
                        break;
                }
            }
        }
        catch (Exception ex)
        {
            LogWrapper.Warn(ex, "Failed to process EasyTier response");
        }
    }
}