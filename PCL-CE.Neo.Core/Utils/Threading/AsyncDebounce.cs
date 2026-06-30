using System;
using System.Threading;
using System.Threading.Tasks;

namespace PCL_CE.Neo.Core.Utils.Threading;

public class AsyncDebounce
{
    private readonly TimeSpan _delay;
    private readonly Action<CancellationToken> _action;
    private CancellationTokenSource _cts = new();
    private readonly object _lock = new();
    private Task? _pendingTask;

    public AsyncDebounce(Action<CancellationToken> action, TimeSpan delay)
    {
        _action = action ?? throw new ArgumentNullException(nameof(action));
        _delay = delay;
    }

    public AsyncDebounce(Func<CancellationToken, Task> asyncAction, TimeSpan delay)
    {
        _action = async ct => await asyncAction(ct);
        _delay = delay;
    }

    public void Invoke()
    {
        lock (_lock)
        {
            _cts.Cancel();
            _cts.Dispose();
            
            var newCts = new CancellationTokenSource();
            _cts = newCts;

            _pendingTask = _ExecuteAsync(newCts.Token);
        }
    }

    public async Task InvokeAsync()
    {
        lock (_lock)
        {
            _cts.Cancel();
            _cts.Dispose();
            
            var newCts = new CancellationTokenSource();
            _cts = newCts;

            _pendingTask = _ExecuteAsync(newCts.Token);
        }

        if (_pendingTask != null)
        {
            await _pendingTask;
        }
    }

    private async Task _ExecuteAsync(CancellationToken cancellationToken)
    {
        try
        {
            await Task.Delay(_delay, cancellationToken);
            await Task.Run(() => _action(cancellationToken), cancellationToken);
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task FlushAsync()
    {
        lock (_lock)
        {
            if (_pendingTask == null)
                return;
        }

        await _pendingTask;
    }

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
    }
}