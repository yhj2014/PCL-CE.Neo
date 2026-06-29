using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace PCL_CE.Neo.Core.Utils.Threading;

public class LimitedTaskPool : IDisposable
{
    private readonly int _maxConcurrency;
    private readonly SemaphoreSlim _semaphore;
    private readonly ConcurrentQueue<Task> _runningTasks = new();
    private bool _isDisposed;

    public LimitedTaskPool(int maxConcurrency = 4)
    {
        if (maxConcurrency < 1)
            throw new ArgumentOutOfRangeException(nameof(maxConcurrency));

        _maxConcurrency = maxConcurrency;
        _semaphore = new SemaphoreSlim(maxConcurrency);
    }

    public Task EnqueueAsync(Func<Task> func)
    {
        if (_isDisposed)
            throw new ObjectDisposedException(nameof(LimitedTaskPool));

        return Task.Run(async () =>
        {
            await _semaphore.WaitAsync();
            try
            {
                var task = func();
                _runningTasks.Enqueue(task);
                await task;
            }
            finally
            {
                _semaphore.Release();
                CleanupCompletedTasks();
            }
        });
    }

    public Task<T> EnqueueAsync<T>(Func<Task<T>> func)
    {
        if (_isDisposed)
            throw new ObjectDisposedException(nameof(LimitedTaskPool));

        return Task.Run(async () =>
        {
            await _semaphore.WaitAsync();
            try
            {
                var task = func();
                _runningTasks.Enqueue(task);
                return await task;
            }
            finally
            {
                _semaphore.Release();
                CleanupCompletedTasks();
            }
        });
    }

    public async Task WaitAllAsync()
    {
        while (_runningTasks.Count > 0)
        {
            if (_runningTasks.TryPeek(out var task))
            {
                await task;
                CleanupCompletedTasks();
            }
            await Task.Delay(10);
        }
    }

    public int PendingCount => _maxConcurrency - _semaphore.CurrentCount;
    public int RunningCount => _runningTasks.Count;

    private void CleanupCompletedTasks()
    {
        while (_runningTasks.TryPeek(out var task) && task.IsCompleted)
        {
            _runningTasks.TryDequeue(out _);
        }
    }

    public void Dispose()
    {
        if (_isDisposed)
            return;

        _isDisposed = true;
        _semaphore.Dispose();
    }
}