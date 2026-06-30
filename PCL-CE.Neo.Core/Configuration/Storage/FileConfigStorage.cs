using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace PCL_CE.Neo.Core.Configuration.Storage;

public class FileConfigStorage : ConfigStorage
{
    private readonly ILogger<FileConfigStorage> _logger;
    public IKeyValueFileProvider File { get; }

    private readonly Channel<(string, Action)> _writeActionChannel;
    private readonly CancellationTokenSource _writeActionCts;
    private readonly ManualResetEventSlim _writeStopEvent = new(true);

    public FileConfigStorage(IKeyValueFileProvider file) : this(file, Microsoft.Extensions.Logging.Abstractions.NullLogger<FileConfigStorage>.Instance) { }

    public FileConfigStorage(IKeyValueFileProvider file, ILogger<FileConfigStorage> logger)
    {
        File = file;
        _logger = logger;
        _writeActionChannel = Channel.CreateUnbounded<(string, Action)>();
        _writeActionCts = new CancellationTokenSource();
        Task.Run(async () =>
        {
            _writeStopEvent.Reset();
            const long syncInterval = 10000;
            var lastSyncTick = 0L;
            var cancelToken = _writeActionCts.Token;
            var writeActionMap = new Dictionary<string, Action>();
            var reader = _writeActionChannel.Reader;
            try
            {
                while (!cancelToken.IsCancellationRequested)
                {
                    var (key, action) = await reader.ReadAsync(cancelToken);
                    writeActionMap[key] = action;
                    if (Environment.TickCount64 - lastSyncTick < syncInterval || cancelToken.IsCancellationRequested) continue;
                    Sync();
                    lastSyncTick = Environment.TickCount64;
                    writeActionMap.Clear();
                }
            }
            catch (OperationCanceledException) { }
            finally
            {
                Sync();
            }
            _writeStopEvent.Set();
            return;

            void Sync()
            {
                try
                {
                    _logger.LogTrace("正在保存 {FilePath}", File.FilePath);
                    foreach (var action in writeActionMap.Values) action();
                    File.Sync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "配置文件保存失败");
                }
            }
        });
    }

    protected override void OnStop()
    {
        _writeActionCts.Cancel();
        _writeStopEvent.Wait();
        _writeStopEvent.Dispose();
    }

    protected override bool OnAccess<TKey, TValue>(
        StorageAction action,
        ref TKey key,
        [NotNullWhen(true)] ref TValue value,
        object? argument)
    {
        if (key is not string strKey) throw new NotSupportedException($"Key '{key}' is not supported");
        switch (action)
        {
            case StorageAction.Get:
                if (!File.Exists(strKey)) return false;
                value = File.Get<TValue>(strKey);
                return true;
            case StorageAction.Exists:
                if (typeof(TValue) == typeof(bool)) Unsafe.As<TValue, bool>(ref value) = File.Exists(strKey);
                else throw new InvalidOperationException($"Storage action '{StorageAction.Exists}' must have a boolean value");
                return true;
            case StorageAction.Set:
                var localValue = value;
                _writeActionChannel.Writer.TryWrite((strKey, () => File.Set(strKey, localValue)));
                return false;
            case StorageAction.Delete:
                _writeActionChannel.Writer.TryWrite((strKey, () => File.Remove(strKey)));
                return false;
            default: throw new InvalidOperationException($"Invalid storage action: {action}");
        }
    }

    public override string ToString() => $"{base.ToString()} ({File.FilePath})";
}