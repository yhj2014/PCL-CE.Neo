using System;
using System.Threading;
using System.Threading.Tasks;

namespace PCL_CE.Neo.Core.Utils.Threading;

public class AsyncManualResetEvent
{
    private volatile TaskCompletionSource<bool> _tcs = new();
    private readonly object _lock = new();

    public bool IsSet
    {
        get
        {
            lock (_lock)
            {
                return _tcs.Task.IsCompleted;
            }
        }
    }

    public void Set()
    {
        lock (_lock)
        {
            if (!_tcs.Task.IsCompleted)
            {
                _tcs.TrySetResult(true);
            }
        }
    }

    public void Reset()
    {
        lock (_lock)
        {
            if (_tcs.Task.IsCompleted)
            {
                _tcs = new TaskCompletionSource<bool>();
            }
        }
    }

    public Task WaitAsync()
    {
        lock (_lock)
        {
            return _tcs.Task;
        }
    }

    public Task WaitAsync(CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromCanceled(cancellationToken);
        }

        var tcs = new TaskCompletionSource<bool>();
        var registration = cancellationToken.Register(() => tcs.TrySetCanceled(cancellationToken));

        WaitAsync().ContinueWith(task =>
        {
            registration.Dispose();
            if (task.IsCompletedSuccessfully)
                tcs.TrySetResult(true);
            else if (task.IsFaulted)
                tcs.TrySetException(task.Exception!.InnerExceptions);
            else if (task.IsCanceled)
                tcs.TrySetCanceled();
        }, TaskScheduler.Default);

        return tcs.Task;
    }

    public bool Wait(int millisecondsTimeout)
    {
        return WaitAsync().Wait(millisecondsTimeout);
    }

    public bool Wait(TimeSpan timeout)
    {
        return WaitAsync().Wait(timeout);
    }

    public void Wait()
    {
        WaitAsync().Wait();
    }
}