using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace PCL_CE.Neo.Core.IO.Net;

public sealed class TcpForward(
    IPAddress listenAddress,
    int listenPort,
    IPAddress targetAddress,
    int targetPort,
    int maxConnections = 10)
    : IDisposable
{
    private Socket? _listenerSocket;
    private CancellationTokenSource? _cts;
    private readonly SemaphoreSlim _connectionSemaphore = new(maxConnections, maxConnections);
    private readonly ConcurrentDictionary<Guid, ConnectionPair> _activeConnections = new();

    private bool _isRunning;

    public int LocalPort { get; private set; }

    public int ActiveConnections => _activeConnections.Count;

    public void Start()
    {
        if (_isRunning) return;

        _cts = new CancellationTokenSource();
        _isRunning = true;

        try
        {
            _listenerSocket = new Socket(SocketType.Stream, ProtocolType.Tcp)
            {
                NoDelay = true,
                ReceiveBufferSize = 8192,
                SendBufferSize = 8192
            };

            _listenerSocket.Bind(new IPEndPoint(listenAddress, listenPort));
            _listenerSocket.Listen(100);

            if (_listenerSocket.LocalEndPoint is not IPEndPoint endPoint) throw new InvalidCastException("出现了意外的转换操作");
            LocalPort = endPoint.Port;

            _ = Task.Run(() => _AcceptConnectionsAsync(_cts.Token), _cts.Token);
        }
        catch (Exception ex)
        {
            _isRunning = false;
            throw;
        }
    }

    public void Stop()
    {
        if (!_isRunning) return;

        _cts?.Cancel();
        _isRunning = false;

        foreach (var connection in _activeConnections.Values)
        {
            connection.ClientSocket.SafeClose();
            connection.TargetSocket.SafeClose();
        }
        _activeConnections.Clear();

        _listenerSocket?.SafeClose();
    }

    private async Task _AcceptConnectionsAsync(CancellationToken cancellationToken)
    {
        cancellationToken.Register(() =>
        {
            _listenerSocket.SafeClose();
        });
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                if (_listenerSocket == null) break;
                var clientSocket = await _listenerSocket.AcceptAsync(cancellationToken);

                if (_activeConnections.Count >= maxConnections)
                {
                    clientSocket.SafeClose();
                    continue;
                }

                await _connectionSemaphore.WaitAsync(cancellationToken);

                _ = Task.Run(() => _HandleConnectionAsync(clientSocket, cancellationToken), cancellationToken)
                    .ContinueWith(_ => _connectionSemaphore.Release(), TaskScheduler.Default);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception)
            {
                await Task.Delay(1000, cancellationToken);
            }
        }
    }

    private async Task _HandleConnectionAsync(Socket clientSocket, CancellationToken cancellationToken)
    {
        var connectionId = Guid.NewGuid();

        try
        {
            var targetSocket = new Socket(SocketType.Stream, ProtocolType.Tcp)
            {
                NoDelay = true,
                ReceiveBufferSize = 8192,
                SendBufferSize = 8192
            };

            await targetSocket.ConnectAsync(targetAddress, targetPort, cancellationToken);

            var connectionPair = new ConnectionPair(clientSocket, targetSocket);
            _activeConnections[connectionId] = connectionPair;

            var forwardTask1 = _ForwardDataAsync(clientSocket, targetSocket, cancellationToken);
            var forwardTask2 = _ForwardDataAsync(targetSocket, clientSocket, cancellationToken);

            await Task.WhenAny(forwardTask1, forwardTask2);
        }
        catch (OperationCanceledException)
        {
        }
        catch
        {
        }
        finally
        {
            clientSocket.SafeClose();
            _activeConnections.TryRemove(connectionId, out _);
        }
    }

    private static async Task _ForwardDataAsync(Socket source, Socket destination, CancellationToken cancellationToken)
    {
        using var bufferOwner = MemoryPool<byte>.Shared.Rent(8192);
        try
        {
            var buffer = bufferOwner.Memory;
            while (!cancellationToken.IsCancellationRequested)
            {
                var bytesRead = await source.ReceiveAsync(buffer, SocketFlags.None, cancellationToken);
                if (bytesRead == 0) break;

                await destination.SendAsync(buffer[..bytesRead], SocketFlags.None, cancellationToken);
            }
        }
        catch { }
    }

    private bool _disposed;

    public void Dispose()
    {
        _Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void _Dispose(bool disposing)
    {
        if (!disposing) return;
        if (_disposed) return;
        Stop();
        _cts?.Dispose();
        _connectionSemaphore.Dispose();
        _disposed = true;
    }

    ~TcpForward()
    {
        _Dispose(false);
    }

    private class ConnectionPair(Socket clientSocket, Socket targetSocket)
    {
        public Socket ClientSocket { get; } = clientSocket;
        public Socket TargetSocket { get; } = targetSocket;
    }
}