using System.Threading;

namespace PCL_CE.Neo.Core.Utils.Threading;

public class AsyncManualResetEvent
{
    private readonly object _lock = new();
    private TaskCompletionSource<bool>? _tcs;
    private bool _isSet;

    public AsyncManualResetEvent(bool initialState = false)
    {
        _isSet = initialState;
        if (_isSet)
        {
            _tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            _tcs.SetResult(true);
        }
    }

    public bool IsSet => _isSet;

    public Task WaitAsync(CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            if (_isSet)
                return Task.CompletedTask;

            if (_tcs == null)
            {
                _tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            }
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromCanceled(cancellationToken);
        }

        return cancellationToken.CanBeCanceled
            ? _tcs!.Task.WaitAsync(cancellationToken)
            : _tcs!.Task;
    }

    public void Set()
    {
        lock (_lock)
        {
            if (_isSet)
                return;

            _isSet = true;
            _tcs?.SetResult(true);
            _tcs = null;
        }
    }

    public void Reset()
    {
        lock (_lock)
        {
            _isSet = false;
            _tcs = null;
        }
    }
}