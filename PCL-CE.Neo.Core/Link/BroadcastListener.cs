using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PCL_CE.Neo.Core.Link;

public interface IBroadcastListener
{
    event Action<BroadcastRecord>? ServerFound;
    Task StartAsync();
    Task StopAsync();
    Task<List<BroadcastRecord>> GetFoundServersAsync();
    bool IsRunning { get; }
}

public class BroadcastListener : IBroadcastListener
{
    private UdpClient? _udpClient;
    private bool _isRunning;
    private readonly List<BroadcastRecord> _foundServers = new();
    private readonly BroadcastOptions _options;
    private CancellationTokenSource? _cts;

    public event Action<BroadcastRecord>? ServerFound;

    public bool IsRunning => _isRunning;

    public BroadcastListener(BroadcastOptions? options = null)
    {
        _options = options ?? new BroadcastOptions();
    }

    public async Task StartAsync()
    {
        try
        {
            _udpClient = new UdpClient(_options.ListenPort);
            _isRunning = true;
            _cts = new CancellationTokenSource();

            _ = ListenLoopAsync(_cts.Token);
            _ = CleanupLoopAsync(_cts.Token);

            LogWrapper.Info($"Broadcast listener started on port {_options.ListenPort}");
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, $"Failed to start broadcast listener on port {_options.ListenPort}");
            throw;
        }
    }

    public async Task StopAsync()
    {
        try
        {
            _cts?.Cancel();
            _isRunning = false;

            if (_udpClient != null)
            {
                _udpClient.Close();
                _udpClient = null;
            }

            LogWrapper.Info("Broadcast listener stopped");
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, "Failed to stop broadcast listener");
            throw;
        }
    }

    public async Task<List<BroadcastRecord>> GetFoundServersAsync()
    {
        await CleanupExpiredRecords();
        return new List<BroadcastRecord>(_foundServers);
    }

    private async Task ListenLoopAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (_isRunning && !cancellationToken.IsCancellationRequested)
            {
                if (_udpClient == null)
                    break;

                var result = await _udpClient.ReceiveAsync(cancellationToken);
                await ProcessBroadcastAsync(result.Buffer, result.RemoteEndPoint);
            }
        }
        catch (OperationCanceledException)
        {
            // Expected when stopping
        }
        catch (Exception ex)
        {
            LogWrapper.Warn(ex, "Broadcast listener loop stopped");
            _isRunning = false;
        }
    }

    private async Task ProcessBroadcastAsync(byte[] data, IPEndPoint remoteEndPoint)
    {
        try
        {
            var message = Encoding.UTF8.GetString(data);
            
            if (message.StartsWith("PCL-CE-NEO::"))
            {
                var parts = message.Split("::");
                if (parts.Length >= 3)
                {
                    var serverAddress = parts[1];
                    var port = int.TryParse(parts[2], out var p) ? p : 25565;

                    var record = new BroadcastRecord
                    {
                        ServerAddress = serverAddress,
                        Port = port,
                        LastSeen = DateTime.Now,
                        IsLocal = true
                    };

                    UpdateOrAddRecord(record);
                    ServerFound?.Invoke(record);

                    LogWrapper.Debug($"Found local server: {serverAddress}:{port}");
                }
            }
        }
        catch (Exception ex)
        {
            LogWrapper.Warn(ex, "Failed to process broadcast message");
        }
    }

    private void UpdateOrAddRecord(BroadcastRecord record)
    {
        lock (_foundServers)
        {
            var existing = _foundServers.FirstOrDefault(r => 
                r.ServerAddress == record.ServerAddress && r.Port == record.Port);

            if (existing != null)
            {
                existing.LastSeen = record.LastSeen;
                existing.IsLocal = record.IsLocal;
            }
            else
            {
                if (_foundServers.Count >= _options.MaxRecords)
                {
                    var oldest = _foundServers.OrderBy(r => r.LastSeen).First();
                    _foundServers.Remove(oldest);
                }
                _foundServers.Add(record);
            }
        }
    }

    private async Task CleanupLoopAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (_isRunning && !cancellationToken.IsCancellationRequested)
            {
                await CleanupExpiredRecords();
                await Task.Delay(_options.Timeout, cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            // Expected when stopping
        }
        catch (Exception ex)
        {
            LogWrapper.Warn(ex, "Broadcast cleanup loop stopped");
        }
    }

    private Task CleanupExpiredRecords()
    {
        try
        {
            lock (_foundServers)
            {
                var expired = _foundServers
                    .Where(r => DateTime.Now - r.LastSeen > _options.Timeout)
                    .ToList();

                foreach (var record in expired)
                {
                    _foundServers.Remove(record);
                }
            }
        }
        catch (Exception ex)
        {
            LogWrapper.Warn(ex, "Failed to cleanup expired broadcast records");
        }

        return Task.CompletedTask;
    }
}