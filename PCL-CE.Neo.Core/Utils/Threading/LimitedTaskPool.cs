using System.Threading.Tasks;

namespace PCL_CE.Neo.Core.Utils.Threading;

public class LimitedTaskPool : IDisposable
{
    private readonly LimitedConcurrencyLevelTaskScheduler _scheduler;
    private readonly TaskFactory _factory;
    private bool _disposed;

    public LimitedTaskPool(int maxConcurrencyLevel)
    {
        _scheduler = new LimitedConcurrencyLevelTaskScheduler(maxConcurrencyLevel);
        _factory = new TaskFactory(_scheduler);
    }

    public int MaxConcurrencyLevel => _scheduler.MaximumConcurrencyLevel;

    public Task StartNew(Action action)
    {
        ThrowIfDisposed();
        return _factory.StartNew(action);
    }

    public Task StartNew(Action<object?> action, object? state)
    {
        ThrowIfDisposed();
        return _factory.StartNew(action, state);
    }

    public Task<TResult> StartNew<TResult>(Func<TResult> function)
    {
        ThrowIfDisposed();
        return _factory.StartNew(function);
    }

    public Task<TResult> StartNew<TResult>(Func<object?, TResult> function, object? state)
    {
        ThrowIfDisposed();
        return _factory.StartNew(function, state);
    }

    public Task RunAsync(Func<Task> function)
    {
        ThrowIfDisposed();
        return _factory.StartNew(function).Unwrap();
    }

    public Task<TResult> RunAsync<TResult>(Func<Task<TResult>> function)
    {
        ThrowIfDisposed();
        return _factory.StartNew(function).Unwrap();
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
            _disposed = true;
        }

        _disposed = true;
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(LimitedTaskPool));
    }
}