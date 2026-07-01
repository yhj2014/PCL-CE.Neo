using System.Threading.Tasks;

namespace PCL_CE.Neo.Core.Utils.Threading;

public class DualThreadPool : IDisposable
{
    private readonly LimitedTaskPool _cpuBoundPool;
    private readonly LimitedTaskPool _ioBoundPool;
    private bool _disposed;

    public DualThreadPool(int cpuBoundConcurrency = 4, int ioBoundConcurrency = 16)
    {
        _cpuBoundPool = new LimitedTaskPool(cpuBoundConcurrency);
        _ioBoundPool = new LimitedTaskPool(ioBoundConcurrency);
    }

    public int CpuBoundConcurrency => _cpuBoundPool.MaxConcurrencyLevel;
    public int IoBoundConcurrency => _ioBoundPool.MaxConcurrencyLevel;

    public Task RunCpuBoundAsync(Action action)
    {
        ThrowIfDisposed();
        return _cpuBoundPool.StartNew(action);
    }

    public Task<TResult> RunCpuBoundAsync<TResult>(Func<TResult> function)
    {
        ThrowIfDisposed();
        return _cpuBoundPool.StartNew(function);
    }

    public Task RunCpuBoundAsync(Func<Task> function)
    {
        ThrowIfDisposed();
        return _cpuBoundPool.RunAsync(function);
    }

    public Task<TResult> RunCpuBoundAsync<TResult>(Func<Task<TResult>> function)
    {
        ThrowIfDisposed();
        return _cpuBoundPool.RunAsync(function);
    }

    public Task RunIoBoundAsync(Action action)
    {
        ThrowIfDisposed();
        return _ioBoundPool.StartNew(action);
    }

    public Task<TResult> RunIoBoundAsync<TResult>(Func<TResult> function)
    {
        ThrowIfDisposed();
        return _ioBoundPool.StartNew(function);
    }

    public Task RunIoBoundAsync(Func<Task> function)
    {
        ThrowIfDisposed();
        return _ioBoundPool.RunAsync(function);
    }

    public Task<TResult> RunIoBoundAsync<TResult>(Func<Task<TResult>> function)
    {
        ThrowIfDisposed();
        return _ioBoundPool.RunAsync(function);
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
            _cpuBoundPool.Dispose();
            _ioBoundPool.Dispose();
        }

        _disposed = true;
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(DualThreadPool));
    }
}