using System;
using System.Threading;
using System.Threading.Tasks;

namespace PCL_CE.Neo.Core.Utils.Threading;

public sealed class AsyncManualResetEvent : IDisposable
{
    private readonly object _syncLock = new();
    private TaskCompletionSource<bool> _tcs = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly ManualResetEventSlim _mre = new(false);
    private bool _disposed;

    public AsyncManualResetEvent(bool initialState = false)
    {
        if (!initialState) return;
        _tcs.SetResult(true);
        _mre.Set();
    }

    public bool IsSet
    {
        get { lock (_syncLock) { return _tcs.Task.IsCompleted; } }
    }

    public Task WaitAsync(CancellationToken cancellationToken = default)
    {
        TaskCompletionSource<bool> t;
        lock (_syncLock) { t = _tcs; }
        if (!cancellationToken.CanBeCanceled || t.Task.IsCompleted) return t.Task;
        return _WaitWithCancellationAsync(t.Task, cancellationToken);
    }

    private static async Task _WaitWithCancellationAsync(Task waitTask, CancellationToken ct)
    {
        var cancelTcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        using (ct.Register(s => ((TaskCompletionSource<bool>)s!).TrySetResult(true), cancelTcs))
        {
            var completed = await Task.WhenAny(waitTask, cancelTcs.Task).ConfigureAwait(false);
            if (completed == cancelTcs.Task) ct.ThrowIfCancellationRequested();
            await waitTask.ConfigureAwait(false);
        }
    }

    public void Wait() => _mre.Wait();
    public bool Wait(int millisecondsTimeout) => _mre.Wait(millisecondsTimeout);
    public bool Wait(TimeSpan timeout) => _mre.Wait(timeout);
    public void Wait(CancellationToken cancellationToken) => _mre.Wait(cancellationToken);

    public void Set()
    {
        lock (_syncLock)
        {
            _tcs.TrySetResult(true);
            _mre.Set();
        }
    }

    public void Reset()
    {
        lock (_syncLock)
        {
            if (!_tcs.Task.IsCompleted) return;
            _tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            _mre.Reset();
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _mre.Dispose();
        _disposed = true;
    }
}