using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PCL_CE.Neo.Core.Utils.Threading;

public sealed class AsyncCountResetEvent : IDisposable
{
    private readonly Queue<TaskCompletionSource<bool>> _waiters = new();
    private readonly object _lock = new();
    private int _permits;
    private bool _disposed;

    ~AsyncCountResetEvent()
    {
        Dispose();
    }

    public Task WaitAsync()
    {
        lock (_lock)
        {
            ObjectDisposedException.ThrowIf(_disposed, nameof(AsyncCountResetEvent));

            if (_permits > 0)
            {
                _permits--;
                return Task.CompletedTask;
            }

            var tcs = new TaskCompletionSource<bool>(
                TaskCreationOptions.RunContinuationsAsynchronously);
            _waiters.Enqueue(tcs);
            return tcs.Task;
        }
    }

    public void Set(int count = 1)
    {
        if (count <= 0) return;

        lock (_lock)
        {
            if (_disposed) return;

            while (count > 0 && _waiters.Count > 0)
            {
                var tcs = _waiters.Dequeue();
                tcs.TrySetResult(true);
                count--;
            }

            _permits += count;
        }
    }

    public void Dispose()
    {
        lock (_lock)
        {
            if (_disposed) return;
            _disposed = true;

            while (_waiters.Count > 0)
            {
                var tcs = _waiters.Dequeue();
                tcs.TrySetException(new ObjectDisposedException(nameof(AsyncCountResetEvent)));
            }
        }

        GC.SuppressFinalize(this);
    }
}