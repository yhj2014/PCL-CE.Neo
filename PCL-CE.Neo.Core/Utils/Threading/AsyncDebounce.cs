using System.Threading;
using System.Threading.Tasks;

namespace PCL_CE.Neo.Core.Utils.Threading;

public class AsyncDebounce
{
    private readonly TimeSpan _delay;
    private readonly Action? _action;
    private readonly Func<Task>? _asyncAction;
    private CancellationTokenSource? _cts;
    private readonly object _lock = new();

    public AsyncDebounce(TimeSpan delay, Action action)
    {
        _delay = delay;
        _action = action;
    }

    public AsyncDebounce(TimeSpan delay, Func<Task> asyncAction)
    {
        _delay = delay;
        _asyncAction = asyncAction;
    }

    public void Invoke()
    {
        lock (_lock)
        {
            _cts?.Cancel();
            _cts = new CancellationTokenSource();

            var cts = _cts;
            Task.Delay(_delay, cts.Token)
                .ContinueWith(async task =>
                {
                    if (!task.IsCanceled)
                    {
                        if (_action != null)
                        {
                            _action();
                        }
                        else if (_asyncAction != null)
                        {
                            await _asyncAction();
                        }
                    }
                }, TaskContinuationOptions.ExecuteSynchronously);
        }
    }

    public void Dispose()
    {
        lock (_lock)
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
        }
    }
}