using System;
using System.Threading;
using System.Threading.Tasks;

namespace PCL_CE.Neo.Core.Utils.Threading;

public class AsyncDebounce
{
    private readonly Func<Task> _action;
    private readonly TimeSpan _delay;
    private CancellationTokenSource? _cts;
    private int _isExecuting;

    public AsyncDebounce(Func<Task> action, TimeSpan delay)
    {
        _action = action ?? throw new ArgumentNullException(nameof(action));
        _delay = delay;
    }

    public Task InvokeAsync()
    {
        Interlocked.Exchange(ref _isExecuting, 1);
        
        _cts?.Cancel();
        _cts = new CancellationTokenSource();
        
        return Task.Delay(_delay, _cts.Token)
            .ContinueWith(async _ =>
            {
                try
                {
                    await _action();
                }
                catch (Exception ex)
                {
                    LogWrapper.Error(ex, "Debounced action failed");
                }
                finally
                {
                    Interlocked.Exchange(ref _isExecuting, 0);
                }
            }, TaskContinuationOptions.NotOnCanceled)
            .Unwrap();
    }

    public void Cancel()
    {
        _cts?.Cancel();
    }

    public bool IsPending => _cts != null && !_cts.IsCancellationRequested;
}