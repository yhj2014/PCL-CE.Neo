using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace PCL_CE.Neo.Core.Utils.Threading;

public class LimitedTaskPool : IDisposable
{
    private readonly int _maxConcurrency;
    private readonly ConcurrentQueue<TaskWrapper> _taskQueue = new();
    private readonly SemaphoreSlim _semaphore;
    private readonly ILogger<LimitedTaskPool> _logger;
    private int _runningTasks;
    private bool _disposed;

    public int MaxConcurrency => _maxConcurrency;
    public int QueuedTasks => _taskQueue.Count;
    public int RunningTasks => Volatile.Read(ref _runningTasks);

    public LimitedTaskPool(int maxConcurrency, ILogger<LimitedTaskPool> logger)
    {
        if (maxConcurrency < 1)
            throw new ArgumentOutOfRangeException(nameof(maxConcurrency), "Max concurrency must be at least 1");

        _maxConcurrency = maxConcurrency;
        _semaphore = new SemaphoreSlim(maxConcurrency);
        _logger = logger;
        _logger.LogInformation("LimitedTaskPool created with max concurrency: {MaxConcurrency}", maxConcurrency);
    }

    public Task<T> Enqueue<T>(Func<T> func)
    {
        ThrowIfDisposed();

        var tcs = new TaskCompletionSource<T>();
        _taskQueue.Enqueue(new TaskWrapper
        {
            Action = async () =>
            {
                try
                {
                    var result = func();
                    tcs.SetResult(result);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            }
        });

        _ProcessQueue();
        return tcs.Task;
    }

    public Task<T> Enqueue<T>(Func<Task<T>> func)
    {
        ThrowIfDisposed();

        var tcs = new TaskCompletionSource<T>();
        _taskQueue.Enqueue(new TaskWrapper
        {
            Action = async () =>
            {
                try
                {
                    var result = await func();
                    tcs.SetResult(result);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            }
        });

        _ProcessQueue();
        return tcs.Task;
    }

    public Task Enqueue(Action action)
    {
        ThrowIfDisposed();

        var tcs = new TaskCompletionSource<bool>();
        _taskQueue.Enqueue(new TaskWrapper
        {
            Action = async () =>
            {
                try
                {
                    action();
                    tcs.SetResult(true);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            }
        });

        _ProcessQueue();
        return tcs.Task;
    }

    public Task Enqueue(Func<Task> func)
    {
        ThrowIfDisposed();

        var tcs = new TaskCompletionSource<bool>();
        _taskQueue.Enqueue(new TaskWrapper
        {
            Action = async () =>
            {
                try
                {
                    await func();
                    tcs.SetResult(true);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            }
        });

        _ProcessQueue();
        return tcs.Task;
    }

    private void _ProcessQueue()
    {
        while (!_disposed && _semaphore.Wait(0))
        {
            if (_taskQueue.TryDequeue(out var wrapper))
            {
                Interlocked.Increment(ref _runningTasks);
                _ExecuteTask(wrapper);
            }
            else
            {
                _semaphore.Release();
                break;
            }
        }
    }

    private async void _ExecuteTask(TaskWrapper wrapper)
    {
        try
        {
            await wrapper.Action();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Task execution failed in LimitedTaskPool");
        }
        finally
        {
            Interlocked.Decrement(ref _runningTasks);
            _semaphore.Release();
            _ProcessQueue();
        }
    }

    public Task WaitForAllAsync()
    {
        return Task.Run(() =>
        {
            while (!_disposed && (QueuedTasks > 0 || RunningTasks > 0))
            {
                Thread.Sleep(10);
            }
        });
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        _semaphore.Dispose();
        _logger.LogInformation("LimitedTaskPool disposed");
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(LimitedTaskPool));
    }

    private class TaskWrapper
    {
        public Func<Task>? Action { get; set; }
    }
}