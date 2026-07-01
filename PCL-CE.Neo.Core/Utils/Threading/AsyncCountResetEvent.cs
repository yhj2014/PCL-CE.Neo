using System.Threading;

namespace PCL_CE.Neo.Core.Utils.Threading;

public class AsyncCountResetEvent
{
    private readonly object _lock = new object();
    private readonly int _initialCount;
    private int _currentCount;
    private TaskCompletionSource<bool>? _tcs;

    public AsyncCountResetEvent(int initialCount)
    {
        if (initialCount < 0)
            throw new ArgumentOutOfRangeException(nameof(initialCount), "初始计数不能为负数。");

        _initialCount = initialCount;
        _currentCount = initialCount;
    }

    public int CurrentCount => Volatile.Read(ref _currentCount);

    public Task WaitAsync()
    {
        lock (_lock)
        {
            if (_currentCount <= 0)
                return Task.CompletedTask;

            _tcs ??= new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            return _tcs.Task;
        }
    }

    public Task WaitAsync(CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            return Task.FromCanceled(cancellationToken);

        lock (_lock)
        {
            if (_currentCount <= 0)
                return Task.CompletedTask;

            _tcs ??= new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            if (!cancellationToken.CanBeCanceled)
                return _tcs.Task;

            var registration = cancellationToken.Register(() =>
            {
                lock (_lock)
                {
                    _tcs?.TrySetCanceled(cancellationToken);
                }
            });

            return WaitWithCancellation(_tcs, registration);
        }
    }

    private async Task WaitWithCancellation(TaskCompletionSource<bool> tcs, CancellationTokenRegistration registration)
    {
        try
        {
            await tcs.Task;
        }
        finally
        {
            registration.Dispose();
        }
    }

    public void Decrement()
    {
        lock (_lock)
        {
            if (_currentCount > 0)
            {
                _currentCount--;
                if (_currentCount <= 0)
                {
                    _tcs?.TrySetResult(true);
                    _tcs = null;
                }
            }
        }
    }

    public void Increment()
    {
        lock (_lock)
        {
            _currentCount++;
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
            throw new ArgumentOutOfRangeException(nameof(newCount), "计数不能为负数。");

        lock (_lock)
        {
            _currentCount = newCount;
            if (_currentCount <= 0)
            {
                _tcs?.TrySetResult(true);
                _tcs = null;
            }
            else
            {
                _tcs = null;
            }
        }
    }
}