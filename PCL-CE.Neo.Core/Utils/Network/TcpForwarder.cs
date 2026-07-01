using System.Net;
using System.Net.Sockets;

namespace PCL_CE.Neo.Core.Utils.Network;

public class TcpForwarder : IDisposable
{
    private readonly TcpListener _listener;
    private readonly string _targetHost;
    private readonly int _targetPort;
    private readonly List<TcpClient> _clients = new List<TcpClient>();
    private bool _isRunning;
    private bool _disposed;

    public event EventHandler<TcpForwardEventArgs>? ClientConnected;
    public event EventHandler<TcpForwardEventArgs>? ClientDisconnected;
    public event EventHandler<TcpForwardErrorEventArgs>? ErrorOccurred;

    public TcpForwarder(IPAddress listenAddress, int listenPort, string targetHost, int targetPort)
    {
        _listener = new TcpListener(listenAddress, listenPort);
        _targetHost = targetHost;
        _targetPort = targetPort;
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        _isRunning = true;
        _listener.Start();

        try
        {
            while (_isRunning && !cancellationToken.IsCancellationRequested)
            {
                TcpClient client = await _listener.AcceptTcpClientAsync(cancellationToken);
                _ = HandleClientAsync(client, cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            ErrorOccurred?.Invoke(this, new TcpForwardErrorEventArgs(ex));
        }
        finally
        {
            Stop();
        }
    }

    public void Stop()
    {
        _isRunning = false;
        _listener.Stop();

        lock (_clients)
        {
            foreach (var client in _clients)
            {
                try
                {
                    client.Close();
                }
                catch
                {
                }
            }
            _clients.Clear();
        }
    }

    private async Task HandleClientAsync(TcpClient client, CancellationToken cancellationToken)
    {
        lock (_clients)
        {
            _clients.Add(client);
        }

        ClientConnected?.Invoke(this, new TcpForwardEventArgs(client.Client.RemoteEndPoint));

        try
        {
            using TcpClient targetClient = new TcpClient();
            await targetClient.ConnectAsync(_targetHost, _targetPort, cancellationToken);

            var sourceStream = client.GetStream();
            var targetStream = targetClient.GetStream();

            var forwardTask1 = CopyStreamAsync(sourceStream, targetStream, cancellationToken);
            var forwardTask2 = CopyStreamAsync(targetStream, sourceStream, cancellationToken);

            await Task.WhenAny(forwardTask1, forwardTask2);
        }
        catch (Exception ex)
        {
            ErrorOccurred?.Invoke(this, new TcpForwardErrorEventArgs(ex));
        }
        finally
        {
            lock (_clients)
            {
                _clients.Remove(client);
            }

            try
            {
                client.Close();
            }
            catch
            {
            }

            ClientDisconnected?.Invoke(this, new TcpForwardEventArgs(client.Client.RemoteEndPoint));
        }
    }

    private async Task CopyStreamAsync(Stream source, Stream destination, CancellationToken cancellationToken)
    {
        var buffer = new byte[8192];
        int bytesRead;

        while ((bytesRead = await source.ReadAsync(buffer, cancellationToken)) > 0)
        {
            await destination.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            Stop();
        }

        _disposed = true;
    }

    ~TcpForwarder()
    {
        Dispose(false);
    }
}

public class TcpForwardEventArgs : EventArgs
{
    public EndPoint? RemoteEndPoint { get; }

    public TcpForwardEventArgs(EndPoint? remoteEndPoint)
    {
        RemoteEndPoint = remoteEndPoint;
    }
}

public class TcpForwardErrorEventArgs : EventArgs
{
    public Exception Exception { get; }

    public TcpForwardErrorEventArgs(Exception exception)
    {
        Exception = exception;
    }
}