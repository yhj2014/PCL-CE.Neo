using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using PCL_CE.Neo.Core.Logging;
using PCL_CE.Neo.Core.Link.McPing;

namespace PCL_CE.Neo.Core.Link.Lobby;

public class LobbyService
{
    private static CancellationTokenSource _lobbyCts = new();
    private static Task? _discoveringTask;
    private static CancellationTokenSource _discoveringCts = new();

    public static LobbyState CurrentState { get; private set; } = LobbyState.Idle;
    public static ObservableCollection<FoundWorld> DiscoveredWorlds { get; } = [];
    public static ObservableCollection<PlayerProfile> Players { get; private set; } = [];
    public static bool IsHost { get; private set; }
    public static string? CurrentLobbyCode { get; private set; }
    public static string? CurrentUserName { get; private set; }

    public static event Action<LobbyState, LobbyState>? StateChanged;
    public static event Action? OnNeedDownloadEasyTier;
    public static event Action? OnUserStopGame;
    public static event Action<long>? OnClientPing;
    public static event Action? OnServerShutDown;
    public static event Action? OnServerStarted;
    public static event Action<Exception>? OnServerException;

    public static async Task InitializeAsync()
    {
        if (CurrentState is not LobbyState.Idle && CurrentState is not LobbyState.Error)
        {
            return;
        }

        _SetState(LobbyState.Initializing);
        try
        {
            LogWrapper.Info("LobbyService", "Lobby service initialized successfully.");

            _SetState(LobbyState.Initialized);
            _ = DiscoverWorldAsync();
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, "LobbyService", "Lobby service initialization failed.");
            _SetState(LobbyState.Error);
        }
    }

    public static async Task DiscoverWorldAsync()
    {
        if (_discoveringCts.IsCancellationRequested)
        {
            return;
        }

        if (CurrentState is not LobbyState.Initialized && CurrentState is not LobbyState.Idle)
        {
            return;
        }

        _SetState(LobbyState.Discovering);
        DiscoveredWorlds.Clear();

        _discoveringTask = Task.Run(async () =>
        {
            var recordedPorts = new HashSet<int>();

            using var udpClient = new UdpClient();
            udpClient.EnableBroadcast = true;
            udpClient.Client.ReceiveTimeout = 3000;

            var endpoint = new IPEndPoint(IPAddress.Broadcast, 4444);
            var requestBytes = Encoding.UTF8.GetBytes("PCL-Lobby-Discovery");
            await udpClient.SendAsync(requestBytes, requestBytes.Length, endpoint);

            var receiveTasks = new List<Task>();
            for (int i = 0; i < 10; i++)
            {
                receiveTasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        var result = await udpClient.ReceiveAsync();
                        var port = result.RemoteEndPoint.Port;
                        if (!recordedPorts.Add(port)) return;

                        using var pinger = new McPingService(new IPEndPoint(IPAddress.Loopback, port));
                        using var cts = new CancellationTokenSource(2000);

                        try
                        {
                            var pingRes = await pinger.PingAsync(cts.Token);
                            if (pingRes == null) return;

                            var worldName = $"{pingRes.Description} / {pingRes.Version.Name} ({port})";
                            DiscoveredWorlds.Add(new FoundWorld(worldName, port));
                        }
                        catch (Exception ex)
                        {
                            LogWrapper.Error(ex, "LobbyService", $"Pinging port {port} failed.");
                        }
                    }
                    catch
                    {
                    }
                }));
            }

            await Task.WhenAll(receiveTasks);
        }, _discoveringCts.Token);

        _SetState(LobbyState.Initialized);
    }

    public static async Task<bool> CreateLobbyAsync(int port, string username)
    {
        await _discoveringCts.CancelAsync();

        _SetState(LobbyState.Creating);
        try
        {
            CurrentUserName = username;
            IsHost = true;

            var code = _GenerateLobbyCode();
            CurrentLobbyCode = code;

            _SetState(LobbyState.Connected);
            LogWrapper.Info("LobbyService", $"Lobby created with code: {code}");

            return true;
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, "LobbyService", "Failed to create lobby.");
            await LeaveLobbyAsync();
            return false;
        }
    }

    public static async Task<bool> JoinLobbyAsync(string lobbyCode, string username)
    {
        await _discoveringCts.CancelAsync();

        _SetState(LobbyState.Joining);

        LogWrapper.Info("LobbyService", $"Try to join lobby {lobbyCode}");

        try
        {
            if (string.IsNullOrEmpty(lobbyCode) || lobbyCode.Length != 6)
            {
                throw new ArgumentException("大厅编号格式不正确");
            }

            CurrentUserName = username;
            CurrentLobbyCode = lobbyCode;
            IsHost = false;

            _SetState(LobbyState.Connected);
            return true;
        }
        catch (ArgumentException codeEx)
        {
            LogWrapper.Error(codeEx, "LobbyService", $"Failed to join lobby {lobbyCode}.");
            await LeaveLobbyAsync();
            return false;
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, "LobbyService", $"Failed to join lobby {lobbyCode}.");
            await LeaveLobbyAsync();
            return false;
        }
    }

    public static async Task LeaveLobbyAsync()
    {
        _SetState(LobbyState.Leaving);

        try
        {
            await _lobbyCts.CancelAsync();

            Players.Clear();
            CurrentLobbyCode = null;
            CurrentUserName = null;
            IsHost = false;

            _lobbyCts = new CancellationTokenSource();
            _SetState(LobbyState.Initialized);

            LogWrapper.Info("LobbyService", "Left lobby and cleaned up resources.");

            _discoveringCts = new CancellationTokenSource();
            _ = DiscoverWorldAsync();
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, "LobbyService", "Failed when leave lobby.");
        }
    }

    private static void _SetState(LobbyState newState)
    {
        var oldState = CurrentState;
        if (oldState == newState)
        {
            return;
        }

        CurrentState = newState;
        LogWrapper.Info("LobbyService", $"Lobby state changed from {oldState} to {newState}");
        StateChanged?.Invoke(oldState, newState);
    }

    private static string _GenerateLobbyCode()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        var random = new Random();
        return new string(Enumerable.Range(0, 6).Select(_ => chars[random.Next(chars.Length)]).ToArray());
    }
}