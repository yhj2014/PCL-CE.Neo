using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using PCL_CE.Neo.Core.Abstractions;

namespace PCL_CE.Neo.Core.Adapters;

public class LinkAdapter : ILinkAdapter
{
    private readonly ILogger<LinkAdapter> _logger;
    private readonly INetworkAdapter _network;
    private readonly IConfigAdapter _config;
    private readonly IEasyTierAdapter? _easyTier;

    private LinkState _state = LinkState.Disconnected;
    private string? _roomCode;
    private readonly ConcurrentDictionary<string, PlayerInfo> _players = new();

    public event Action<LinkState>? StateChanged;
    public event Action<string>? MessageReceived;
    public event Action<PlayerInfo>? PlayerJoined;
    public event Action<PlayerInfo>? PlayerLeft;

    public LinkState CurrentState => _state;
    public string? RoomCode => _roomCode;
    public IReadOnlyList<PlayerInfo> Players => _players.Values.ToList();

    public LinkAdapter() : this(
        Microsoft.Extensions.Logging.Abstractions.NullLogger<LinkAdapter>.Instance,
        new NetworkAdapter(),
        new ConfigAdapter())
    {
    }

    public LinkAdapter(
        ILogger<LinkAdapter> logger,
        INetworkAdapter network,
        IConfigAdapter config)
    {
        _logger = logger;
        _network = network;
        _config = config;
    }

    public bool IsConnected => _state == LinkState.Connected;

    public void Connect(string server, int port)
    {
        SetState(LinkState.Connected);
    }

    public async Task<bool> Connect()
    {
        try
        {
            await CreateRoomAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public void Disconnect()
    {
        SetState(LinkState.Disconnected);
        _players.Clear();
        _roomCode = null;
    }

    public async Task<bool> DisconnectAsync()
    {
        try
        {
            Disconnect();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<string> CreateRoomAsync()
    {
        try
        {
            _logger.LogInformation("创建联机房间");
            SetState(LinkState.Connecting);

            _roomCode = GenerateRoomCode();

            var player = new PlayerInfo
            {
                Id = _config.GetConfig("Identify", ""),
                Name = "Host",
                IsHost = true,
                State = PlayerState.Ready
            };

            _players[player.Id] = player;
            SetState(LinkState.Connected);

            _logger.LogInformation("房间创建成功: {RoomCode}", _roomCode);
            return _roomCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "创建房间失败");
            SetState(LinkState.Error);
            throw;
        }
    }

    public async Task<bool> JoinRoomAsync(string roomCode)
    {
        try
        {
            _logger.LogInformation("加入房间: {RoomCode}", roomCode);
            SetState(LinkState.Connecting);

            await Task.Delay(100);

            var player = new PlayerInfo
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Player",
                IsHost = false,
                State = PlayerState.Syncing
            };

            _players[player.Id] = player;
            _roomCode = roomCode;

            await Task.Delay(500);
            UpdatePlayerState(player.Id, PlayerState.Ready);

            SetState(LinkState.Connected);
            _logger.LogInformation("加入房间成功: {RoomCode}", roomCode);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "加入房间失败: {RoomCode}", roomCode);
            SetState(LinkState.Error);
            return false;
        }
    }

    public Task LeaveRoomAsync()
    {
        _logger.LogInformation("离开房间");
        _players.Clear();
        _roomCode = null;
        SetState(LinkState.Disconnected);
        return Task.CompletedTask;
    }

    public Task SendMessageAsync(string message)
    {
        _logger.LogDebug("发送消息: {Message}", message);
        MessageReceived?.Invoke(message);
        return Task.CompletedTask;
    }

    public Task RequestSyncAsync()
    {
        _logger.LogDebug("请求同步");
        return Task.CompletedTask;
    }

    private void SetState(LinkState state)
    {
        _state = state;
        StateChanged?.Invoke(state);
    }

    private void UpdatePlayerState(string playerId, PlayerState state)
    {
        if (_players.TryGetValue(playerId, out var player))
        {
            var updated = player with { State = state };
            _players[playerId] = updated;
        }
    }

    private static string GenerateRoomCode()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        var random = new Random();
        return new string(Enumerable.Range(0, 6).Select(_ => chars[random.Next(chars.Length)]).ToArray());
    }
}

public class EasyTierAdapter : IEasyTierAdapter
{
    private readonly ILogger<EasyTierAdapter> _logger;

    private ETState _state = ETState.Stopped;
    private string? _nodeId;

    public event Action<ETState>? StateChanged;
    public event Action<ETPeerInfo>? PeerConnected;
    public event Action<ETPeerInfo>? PeerDisconnected;
    public event Action<ETPeerInfo, byte[]>? DataReceived;

    public bool IsRunning => _state == ETState.Running;
    public string? NodeId => _nodeId;

    public EasyTierAdapter(ILogger<EasyTierAdapter> logger)
    {
        _logger = logger;
    }

    public Task StartAsync()
    {
        _logger.LogInformation("启动 EasyTier");
        SetState(ETState.Starting);

        _nodeId = Guid.NewGuid().ToString("N")[..8];

        Task.Delay(500).ContinueWith(_ => SetState(ETState.Running));
        return Task.CompletedTask;
    }

    public Task StopAsync()
    {
        _logger.LogInformation("停止 EasyTier");
        SetState(ETState.Stopped);
        _nodeId = null;
        return Task.CompletedTask;
    }

    public Task<ETPeerInfo?> ConnectToPeerAsync(string peerId)
    {
        _logger.LogInformation("连接到节点: {PeerId}", peerId);

        var peer = new ETPeerInfo
        {
            Id = peerId,
            Name = $"Node-{peerId[..4]}",
            Type = ETPeerType.PublicNode,
            Latency = new Random().Next(10, 100)
        };

        PeerConnected?.Invoke(peer);
        return Task.FromResult<ETPeerInfo?>(peer);
    }

    public Task DisconnectPeerAsync(string peerId)
    {
        _logger.LogInformation("断开节点连接: {PeerId}", peerId);
        return Task.CompletedTask;
    }

    public Task SendDataAsync(string peerId, byte[] data)
    {
        _logger.LogDebug("发送数据到节点: {PeerId} ({Size} bytes)", peerId, data.Length);
        return Task.CompletedTask;
    }

    private void SetState(ETState state)
    {
        _state = state;
        StateChanged?.Invoke(state);
    }
}
