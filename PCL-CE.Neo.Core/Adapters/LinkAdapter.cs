using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using PCL_CE.Neo.Core.Abstractions;

namespace PCL_CE.Neo.Core.Adapters;

public class LinkAdapter : ILinkAdapter
{
    private readonly ILogger<LinkAdapter> _logger;
    private readonly INetworkAdapter _network;
    private readonly IConfigAdapter _config;
    private readonly IPathsAdapter _paths;
    private readonly IEasyTierAdapter? _easyTier;

    private LinkState _state = LinkState.Disconnected;
    private string? _roomCode;
    private bool _isHost;
    private readonly ConcurrentDictionary<string, PlayerInfo> _players = new();
    private readonly ConcurrentDictionary<string, byte[]> _playerData = new();
    private CancellationTokenSource? _syncCts;

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
        new ConfigAdapter(),
        new PathsAdapter(),
        null)
    {
    }

    public LinkAdapter(
        ILogger<LinkAdapter> logger,
        INetworkAdapter network,
        IConfigAdapter config,
        IPathsAdapter paths,
        IEasyTierAdapter? easyTier)
    {
        _logger = logger;
        _network = network;
        _config = config;
        _paths = paths;
        _easyTier = easyTier;
    }

    public bool IsConnected => _state == LinkState.Connected;

    public void Connect(string server, int port)
    {
        try
        {
            _logger.LogInformation("连接到服务器: {Server}:{Port}", server, port);
            SetState(LinkState.Connecting);

            // 尝试连接逻辑
            // 这里是简化实现，实际需要通过 EasyTier 建立连接
            SetState(LinkState.Connected);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "连接失败");
            SetState(LinkState.Error);
        }
    }

    public async Task<bool> Connect()
    {
        try
        {
            _logger.LogInformation("初始化联机连接");
            SetState(LinkState.Connecting);

            // 如果有 EasyTier 可用，启动它
            if (_easyTier != null)
            {
                _easyTier.StateChanged += OnEasyTierStateChanged;
                _easyTier.PeerConnected += OnPeerConnected;
                _easyTier.PeerDisconnected += OnPeerDisconnected;
                _easyTier.DataReceived += OnDataReceived;

                await _easyTier.StartAsync();
                _logger.LogInformation("EasyTier 已启动");
            }

            SetState(LinkState.Connected);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "连接初始化失败");
            SetState(LinkState.Error);
            return false;
        }
    }

    public void Disconnect()
    {
        try
        {
            _logger.LogInformation("断开联机连接");

            _syncCts?.Cancel();
            _syncCts?.Dispose();
            _syncCts = null;

            // 停止 EasyTier
            if (_easyTier != null && _easyTier.IsRunning)
            {
                _easyTier.StateChanged -= OnEasyTierStateChanged;
                _easyTier.PeerConnected -= OnPeerConnected;
                _easyTier.PeerDisconnected -= OnPeerDisconnected;
                _easyTier.DataReceived -= OnDataReceived;
                _ = _easyTier.StopAsync();
            }

            // 通知所有玩家离开
            foreach (var player in _players.Values)
            {
                PlayerLeft?.Invoke(player);
            }

            _players.Clear();
            _playerData.Clear();
            _roomCode = null;
            _isHost = false;

            SetState(LinkState.Disconnected);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "断开连接时发生错误");
            SetState(LinkState.Error);
        }
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

            // 生成房间代码
            _roomCode = GenerateRoomCode();
            _isHost = true;

            // 创建主机玩家
            var hostId = GetPlayerId();
            var hostPlayer = new PlayerInfo
            {
                Id = hostId,
                Name = GetPlayerName(),
                IsHost = true,
                State = PlayerState.Ready,
                Latency = 0
            };

            _players[hostId] = hostPlayer;
            PlayerJoined?.Invoke(hostPlayer);

            // 如果有 EasyTier，建立网络
            if (_easyTier != null)
            {
                try
                {
                    await _easyTier.StartAsync();
                    _logger.LogInformation("EasyTier 网络已建立");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "启动 EasyTier 失败，将使用简化模式");
                }
            }

            // 启动同步循环
            StartSyncLoop();

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

            // 验证房间代码格式
            if (string.IsNullOrEmpty(roomCode) || roomCode.Length != 6)
            {
                _logger.LogWarning("无效的房间代码: {RoomCode}", roomCode);
                SetState(LinkState.Error);
                return false;
            }

            _roomCode = roomCode.ToUpperInvariant();
            _isHost = false;

            // 如果有 EasyTier，连接到网络
            if (_easyTier != null)
            {
                try
                {
                    await _easyTier.StartAsync();

                    // 尝试连接到大厅
                    var lobbyInfo = ParseLobbyCode(roomCode);
                    if (lobbyInfo != null)
                    {
                        var peer = await _easyTier.ConnectToPeerAsync(lobbyInfo.RelayServer);
                        if (peer != null)
                        {
                            _logger.LogInformation("已连接到房间网络");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "连接房间网络失败，将使用简化模式");
                }
            }

            // 创建玩家
            var playerId = GetPlayerId();
            var player = new PlayerInfo
            {
                Id = playerId,
                Name = GetPlayerName(),
                IsHost = false,
                State = PlayerState.Syncing,
                Latency = new Random().Next(20, 100)
            };

            _players[playerId] = player;
            PlayerJoined?.Invoke(player);

            // 模拟同步完成
            await Task.Delay(500);
            UpdatePlayerState(playerId, PlayerState.Ready);

            // 启动同步循环
            StartSyncLoop();

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

    public async Task LeaveRoomAsync()
    {
        try
        {
            _logger.LogInformation("离开房间");

            _syncCts?.Cancel();
            _syncCts?.Dispose();
            _syncCts = null;

            // 通知其他玩家
            var playerId = GetPlayerId();
            if (_players.TryRemove(playerId, out var player))
            {
                PlayerLeft?.Invoke(player);
            }

            // 断开 EasyTier 连接
            if (_easyTier != null && !_isHost)
            {
                try
                {
                    await _easyTier.StopAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "停止 EasyTier 时发生错误");
                }
            }

            _players.Clear();
            _playerData.Clear();
            _roomCode = null;
            _isHost = false;

            SetState(LinkState.Disconnected);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "离开房间时发生错误");
        }
    }

    public Task SendMessageAsync(string message)
    {
        try
        {
            if (string.IsNullOrEmpty(message))
            {
                return Task.CompletedTask;
            }

            _logger.LogDebug("发送消息: {Message}", message);

            // 广播消息给所有玩家
            MessageReceived?.Invoke(message);

            // 如果有 EasyTier，通过网络发送
            if (_easyTier != null && _easyTier.IsRunning)
            {
                var data = Encoding.UTF8.GetBytes(message);
                foreach (var peerId in _players.Keys)
                {
                    if (peerId != GetPlayerId())
                    {
                        _ = _easyTier.SendDataAsync(peerId, data);
                    }
                }
            }

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "发送消息失败");
            return Task.CompletedTask;
        }
    }

    public Task RequestSyncAsync()
    {
        try
        {
            _logger.LogDebug("请求同步");

            // 发送本地玩家状态给所有远程玩家
            var playerId = GetPlayerId();
            if (_players.TryGetValue(playerId, out var player))
            {
                var syncData = new SyncMessage
                {
                    Type = "player_sync",
                    PlayerId = player.Id,
                    PlayerName = player.Name,
                    State = player.State,
                    IsHost = player.IsHost
                };

                var json = JsonSerializer.Serialize(syncData);
                var data = Encoding.UTF8.GetBytes(json);

                if (_easyTier != null && _easyTier.IsRunning)
                {
                    foreach (var peerId in _players.Keys)
                    {
                        if (peerId != playerId)
                        {
                            _ = _easyTier.SendDataAsync(peerId, data);
                        }
                    }
                }
            }

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "请求同步失败");
            return Task.CompletedTask;
        }
    }

    private void StartSyncLoop()
    {
        _syncCts?.Cancel();
        _syncCts?.Dispose();
        _syncCts = new CancellationTokenSource();

        _ = Task.Run(async () =>
        {
            while (!_syncCts.Token.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(5000, _syncCts.Token);

                    if (_state != LinkState.Connected)
                    {
                        break;
                    }

                    // 定期同步
                    await RequestSyncAsync();

                    // 更新玩家延迟（模拟）
                    foreach (var player in _players.Values)
                    {
                        if (!player.IsHost)
                        {
                            var updated = player with { Latency = new Random().Next(20, 150) };
                            _players[player.Id] = updated;
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "同步循环发生错误");
                }
            }
        }, _syncCts.Token);
    }

    private void OnEasyTierStateChanged(ETState state)
    {
        _logger.LogDebug("EasyTier 状态变更: {State}", state);

        if (state == ETState.Error)
        {
            SetState(LinkState.Error);
        }
    }

    private void OnPeerConnected(ETPeerInfo peer)
    {
        _logger.LogInformation("节点已连接: {PeerId}", peer.Id);

        var player = new PlayerInfo
        {
            Id = peer.Id,
            Name = peer.Name,
            IsHost = false,
            State = PlayerState.Connected,
            Latency = peer.Latency
        };

        _players[peer.Id] = player;
        PlayerJoined?.Invoke(player);
    }

    private void OnPeerDisconnected(ETPeerInfo peer)
    {
        _logger.LogInformation("节点已断开: {PeerId}", peer.Id);

        if (_players.TryRemove(peer.Id, out var player))
        {
            PlayerLeft?.Invoke(player);
        }
    }

    private void OnDataReceived(ETPeerInfo peer, byte[] data)
    {
        try
        {
            var json = Encoding.UTF8.GetString(data);
            var message = JsonSerializer.Deserialize<SyncMessage>(json);

            if (message == null)
            {
                return;
            }

            switch (message.Type)
            {
                case "player_sync":
                    HandlePlayerSync(message);
                    break;

                case "chat":
                    MessageReceived?.Invoke(message.Content ?? "");
                    break;

                default:
                    _logger.LogDebug("收到未知类型的同步消息: {Type}", message.Type);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "处理收到的数据时发生错误");
        }
    }

    private void HandlePlayerSync(SyncMessage message)
    {
        if (string.IsNullOrEmpty(message.PlayerId))
        {
            return;
        }

        var existingPlayer = _players.GetValueOrDefault(message.PlayerId);
        if (existingPlayer == null)
        {
            // 新玩家加入
            var player = new PlayerInfo
            {
                Id = message.PlayerId,
                Name = message.PlayerName ?? "Unknown",
                IsHost = message.IsHost,
                State = message.State
            };
            _players[message.PlayerId] = player;
            PlayerJoined?.Invoke(player);
        }
        else
        {
            // 更新现有玩家
            var updated = existingPlayer with
            {
                State = message.State,
                Latency = existingPlayer.Latency
            };
            _players[message.PlayerId] = updated;
        }
    }

    private void SetState(LinkState state)
    {
        if (_state != state)
        {
            _state = state;
            _logger.LogDebug("联机状态变更: {State}", state);
            StateChanged?.Invoke(state);
        }
    }

    private void UpdatePlayerState(string playerId, PlayerState state)
    {
        if (_players.TryGetValue(playerId, out var player))
        {
            var updated = player with { State = state };
            _players[playerId] = updated;
        }
    }

    private string GetPlayerId()
    {
        return _config.GetConfig("Identify", "") ?? Guid.NewGuid().ToString("N")[..8];
    }

    private string GetPlayerName()
    {
        var name = _config.GetConfig("LinkUsername", "");
        if (string.IsNullOrEmpty(name))
        {
            name = _config.GetConfig("NickName", "Player");
        }
        return name ?? "Player";
    }

    private static string GenerateRoomCode()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        var random = new Random();
        return new string(Enumerable.Range(0, 6).Select(_ => chars[random.Next(chars.Length)]).ToArray());
    }

    private static LobbyCodeInfo? ParseLobbyCode(string code)
    {
        // 解析大厅代码，提取网络信息
        // 这是一个简化实现，实际需要从服务器获取大厅信息
        try
        {
            // 大厅代码格式: 6位字母数字
            // 实际实现中，需要通过服务器查询实际的连接信息
            return new LobbyCodeInfo
            {
                Code = code,
                RelayServer = "public.easytier.top",
                Port = 11010
            };
        }
        catch
        {
            return null;
        }
    }

    private class SyncMessage
    {
        public string Type { get; set; } = "";
        public string? PlayerId { get; set; }
        public string? PlayerName { get; set; }
        public PlayerState State { get; set; }
        public bool IsHost { get; set; }
        public string? Content { get; set; }
    }

    private class LobbyCodeInfo
    {
        public string Code { get; set; } = "";
        public string RelayServer { get; set; } = "";
        public int Port { get; set; }
    }
}

public class EasyTierAdapter : IEasyTierAdapter
{
    private readonly ILogger<EasyTierAdapter> _logger;
    private readonly string _easyTierPath;

    private ETState _state = ETState.Stopped;
    private string? _nodeId;
    private readonly List<ETPeerInfo> _connectedPeers = new();

    public event Action<ETState>? StateChanged;
    public event Action<ETPeerInfo>? PeerConnected;
    public event Action<ETPeerInfo>? PeerDisconnected;
    public event Action<ETPeerInfo, byte[]>? DataReceived;

    public bool IsRunning => _state == ETState.Running;
    public string? NodeId => _nodeId;

    public EasyTierAdapter() : this(
        Microsoft.Extensions.Logging.Abstractions.NullLogger<EasyTierAdapter>.Instance,
        GetDefaultEasyTierPath())
    {
    }

    public EasyTierAdapter(ILogger<EasyTierAdapter> logger, string easyTierPath)
    {
        _logger = logger;
        _easyTierPath = easyTierPath;
    }

    public EasyTierAdapter(ILogger<EasyTierAdapter> logger)
    {
        _logger = logger;
        _easyTierPath = GetDefaultEasyTierPath();
    }

    public Task StartAsync()
    {
        try
        {
            _logger.LogInformation("启动 EasyTier，路径: {Path}", _easyTierPath);
            SetState(ETState.Starting);

            _nodeId = Guid.NewGuid().ToString("N")[..8];

            // 检查 EasyTier 是否可用
            if (!IsEasyTierAvailable())
            {
                _logger.LogWarning("EasyTier 不可用，将以模拟模式运行");
            }

            // 模拟启动完成
            Task.Delay(500).ContinueWith(_ =>
            {
                if (_state == ETState.Starting)
                {
                    SetState(ETState.Running);
                    _logger.LogInformation("EasyTier 已启动，节点 ID: {NodeId}", _nodeId);
                }
            });

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "启动 EasyTier 失败");
            SetState(ETState.Error);
            throw;
        }
    }

    public Task StopAsync()
    {
        try
        {
            _logger.LogInformation("停止 EasyTier");

            // 断开所有连接的节点
            foreach (var peer in _connectedPeers.ToList())
            {
                _connectedPeers.Remove(peer);
                PeerDisconnected?.Invoke(peer);
            }

            SetState(ETState.Stopped);
            _nodeId = null;

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "停止 EasyTier 失败");
            return Task.CompletedTask;
        }
    }

    public Task<ETPeerInfo?> ConnectToPeerAsync(string peerId)
    {
        try
        {
            _logger.LogInformation("连接到节点: {PeerId}", peerId);

            // 检查 EasyTier 是否可用
            if (!IsEasyTierAvailable())
            {
                _logger.LogWarning("EasyTier 不可用，模拟连接");
            }

            var peer = new ETPeerInfo
            {
                Id = peerId,
                Name = $"Node-{peerId[..Math.Min(4, peerId.Length)]}",
                Type = ETPeerType.PublicNode,
                Latency = new Random().Next(10, 100)
            };

            _connectedPeers.Add(peer);
            PeerConnected?.Invoke(peer);

            return Task.FromResult<ETPeerInfo?>(peer);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "连接节点失败: {PeerId}", peerId);
            return Task.FromResult<ETPeerInfo?>(null);
        }
    }

    public Task DisconnectPeerAsync(string peerId)
    {
        try
        {
            _logger.LogInformation("断开节点连接: {PeerId}", peerId);

            var peer = _connectedPeers.FirstOrDefault(p => p.Id == peerId);
            if (peer != null)
            {
                _connectedPeers.Remove(peer);
                PeerDisconnected?.Invoke(peer);
            }

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "断开节点失败: {PeerId}", peerId);
            return Task.CompletedTask;
        }
    }

    public Task SendDataAsync(string peerId, byte[] data)
    {
        try
        {
            _logger.LogDebug("发送数据到节点: {PeerId} ({Size} bytes)", peerId, data.Length);

            // 模拟数据发送
            // 实际实现中，需要通过 EasyTier 网络发送数据

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "发送数据失败: {PeerId}", peerId);
            return Task.CompletedTask;
        }
    }

    private void SetState(ETState state)
    {
        if (_state != state)
        {
            _state = state;
            StateChanged?.Invoke(state);
        }
    }

    private bool IsEasyTierAvailable()
    {
        // 检查 EasyTier 可执行文件是否存在
        // 这是简化实现，实际需要检查对应平台的二进制文件
        var executableName = OperatingSystem.IsWindows() ? "easytier-core.exe" : "easytier-core";
        var fullPath = Path.Combine(_easyTierPath, executableName);

        return File.Exists(fullPath);
    }

    private static string GetDefaultEasyTierPath()
    {
        // 根据平台返回默认的 EasyTier 路径
        if (OperatingSystem.IsWindows())
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "PCL-CE", "EasyTier");
        }
        else if (OperatingSystem.IsMacOS())
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "PCL-CE", "EasyTier");
        }
        else if (OperatingSystem.IsLinux())
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "pcl-ce", "easytier");
        }

        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "PCL-CE", "EasyTier");
    }
}