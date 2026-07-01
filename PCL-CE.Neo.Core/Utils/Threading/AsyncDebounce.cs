using System.Threading;
using System.Threading.Tasks;

namespace PCL_CE.Neo.Core.Utils.Threading;

public class AsyncDebounce
{
    private readonly TimeSpan _delay;
    private CancellationTokenSource _cts = new CancellationTokenSource();
    private Task? _pendingTask;
    private readonly object _lock = new object();

    public AsyncDebounce(TimeSpan delay)
    {
        _delay = delay;
    }

    public Task InvokeAsync(Func<Task> action)
    {
        lock (_lock)
        {
            _cts.Cancel();
            _cts.Dispose();
            var newCts = new CancellationTokenSource();
            _cts = newCts;

            _pendingTask = InvokeAfterDelay(action, newCts.Token);
            return _pendingTask;
        }
    }

    public Task<TResult> InvokeAsync<TResult>(Func<Task<TResult>> action)
    {
        lock (_lock)
        {
            _cts.Cancel();
            _cts.Dispose();
            var newCts = new CancellationTokenSource();
            _cts = newCts;

            _pendingTask = InvokeAfterDelay(action, newCts.Token);
            return _pendingTask as Task<TResult> ?? Task.FromResult(default(TResult)!);
        }
    }

    public void Invoke(Action action)
    {
        lock (_lock)
        {
            _cts.Cancel();
            _cts.Dispose();
            var newCts = new CancellationTokenSource();
            _cts = newCts;

            _pendingTask = InvokeAfterDelay(action, newCts.Token);
        }
    }

    public Task<TResult> InvokeAsync<TResult>(Func<TResult> action)
    {
        lock (_lock)
        {
            _cts.Cancel();
            _cts.Dispose();
            var newCts = new CancellationTokenSource();
            _cts = newCts;

            _pendingTask = InvokeAfterDelay(action, newCts.Token);
            return _pendingTask as Task<TResult> ?? Task.FromResult(default(TResult)!);
        }
    }

    private async Task InvokeAfterDelay(Func<Task> action, CancellationToken cancellationToken)
    {
        await Task.Delay(_delay, cancellationToken).ConfigureAwait(false);
        if (!cancellationToken.IsCancellationRequested)
            await action().ConfigureAwait(false);
    }

    private async Task<TResult> InvokeAfterDelay<TResult>(Func<Task<TResult>> action, CancellationToken cancellationToken)
    {
        await Task.Delay(_delay, cancellationToken).ConfigureAwait(false);
        return cancellationToken.IsCancellationRequested ? default! : await action().ConfigureAwait(false);
    }

    private async Task InvokeAfterDelay(Action action, CancellationToken cancellationToken)
    {
        await Task.Delay(_delay, cancellationToken).ConfigureAwait(false);
        if (!cancellationToken.IsCancellationRequested)
            action();
    }

    private async Task<TResult> InvokeAfterDelay<TResult>(Func<TResult> action, CancellationToken cancellationToken)
    {
        await Task.Delay(_delay, cancellationToken).ConfigureAwait(false);
        return cancellationToken.IsCancellationRequested ? default! : action();
    }

    public async Task WaitAsync()
    {
        Task? pending;
        lock (_lock)
        {
            pending = _pendingTask;
        }

        if (pending != null)
            await pending;
    }

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
    }
}