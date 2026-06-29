using System;
using System.Threading;
using System.Threading.Tasks;

namespace PCL_CE.Neo.Core.Utils.Threading;

public class AsyncCountResetEvent
{
    private readonly int _initialCount;
    private int _currentCount;
    private TaskCompletionSource<bool>? _tcs;
    private readonly object _lock = new();

    public AsyncCountResetEvent(int initialCount)
    {
        if (initialCount < 0)
            throw new ArgumentOutOfRangeException(nameof(initialCount));

        _initialCount = initialCount;
        _currentCount = initialCount;
    }

    public Task WaitAsync()
    {
        lock (_lock)
        {
            if (_currentCount <= 0)
                return Task.CompletedTask;

            _tcs ??= new TaskCompletionSource<bool>();
            return _tcs.Task;
        }
    }

    public Task WaitAsync(CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            return Task.FromCanceled(cancellationToken);

        var task = WaitAsync();
        if (task.IsCompleted)
            return task;

        return task.WaitAsync(cancellationToken);
    }

    public void Signal()
    {
        lock (_lock)
        {
            if (_currentCount > 0)
                _currentCount--;

            if (_currentCount <= 0 && _tcs != null)
            {
                _tcs.TrySetResult(true);
                _tcs = null;
            }
        }
    }

    public void Reset()
    {
        lock (_lock)
        {
            _currentCount = _initialCount;
            _tcs = null;
        }
    }

    public void Reset(int newCount)
    {
        if (newCount < 0)
            throw new ArgumentOutOfRangeException(nameof(newCount));

        lock (_lock)
        {
            _initialCount = newCount;
            _currentCount = newCount;
            _tcs = null;
        }
    }

    public int CurrentCount => Volatile.Read(ref _currentCount);
}