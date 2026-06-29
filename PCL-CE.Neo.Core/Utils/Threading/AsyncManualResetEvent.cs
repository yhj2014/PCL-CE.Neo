using System;
using System.Threading;
using System.Threading.Tasks;

namespace PCL_CE.Neo.Core.Utils.Threading;

public class AsyncManualResetEvent
{
    private volatile TaskCompletionSource<bool> _tcs = new();
    private readonly object _lock = new();

    public Task WaitAsync()
    {
        return _tcs.Task;
    }

    public Task WaitAsync(CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            return Task.FromCanceled(cancellationToken);

        var tcs = _tcs;
        return tcs.Task.WaitAsync(cancellationToken);
    }

    public void Set()
    {
        lock (_lock)
        {
            _tcs.TrySetResult(true);
        }
    }

    public void Reset()
    {
        lock (_lock)
        {
            if (_tcs.Task.IsCompleted)
                _tcs = new TaskCompletionSource<bool>();
        }
    }

    public bool IsSet => _tcs.Task.IsCompleted;
}