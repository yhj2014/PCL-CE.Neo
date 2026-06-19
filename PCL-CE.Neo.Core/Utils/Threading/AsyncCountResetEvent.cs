using System.Threading;

namespace PCL_CE.Neo.Core.Utils.Threading;

public class AsyncCountResetEvent
{
    private readonly SemaphoreSlim _semaphore;
    private readonly int _initialCount;

    public AsyncCountResetEvent(int initialCount)
    {
        _initialCount = initialCount;
        _semaphore = new SemaphoreSlim(initialCount, initialCount);
    }

    public int CurrentCount => _semaphore.CurrentCount;

    public Task WaitAsync(CancellationToken cancellationToken = default)
    {
        return _semaphore.WaitAsync(cancellationToken);
    }

    public bool Wait(TimeSpan timeout)
    {
        return _semaphore.Wait(timeout);
    }

    public void Wait()
    {
        _semaphore.Wait();
    }

    public void Signal()
    {
        if (_semaphore.CurrentCount < _initialCount)
        {
            _semaphore.Release();
        }
    }

    public void Reset()
    {
        while (_semaphore.CurrentCount > 0)
        {
            try
            {
                _semaphore.Wait(TimeSpan.Zero);
            }
            catch
            {
                break;
            }
        }
    }

    public void Dispose()
    {
        _semaphore.Dispose();
    }
}