using System.Threading;
using System.Threading.Tasks;

namespace PCL_CE.Neo.Core.Utils.Threading;

public class AsyncManualResetEvent
{
    private volatile TaskCompletionSource<bool> _tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

    public Task WaitAsync(CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return Task.FromCanceled(cancellationToken);

        var tcs = _tcs;
        if (cancellationToken.CanBeCanceled)
        {
            return WaitAsyncWithCancellation(tcs, cancellationToken);
        }

        return tcs.Task;
    }

    private async Task WaitAsyncWithCancellation(TaskCompletionSource<bool> tcs, CancellationToken cancellationToken)
    {
        using var registration = cancellationToken.Register(() => tcs.TrySetCanceled(cancellationToken));
        await tcs.Task;
    }

    public void Set()
    {
        _tcs.TrySetResult(true);
    }

    public void Reset()
    {
        var tcs = _tcs;
        if (!tcs.Task.IsCompleted)
            return;

        Interlocked.CompareExchange(ref _tcs, new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously), tcs);
    }

    public bool IsSet => _tcs.Task.IsCompletedSuccessfully;
}