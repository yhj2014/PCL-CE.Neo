using System;
using System.Threading;
using System.Threading.Tasks;

namespace PCL_CE.Neo.Core.Utils.Threading;

public class AsyncDebounce(CancellationToken cancelToken = default) : IDisposable
{
    public required TimeSpan Delay { get; init; }

    public required Func<Task> ScheduledTask { get; init; }

    public bool IsCurrentTaskCompleted { get; private set; }

    public bool IsCurrentTaskRunning => _currentTask != null;

    private Task? _currentTask;
    private Task? _worker;
    private CancellationTokenSource? _currentDelayCts;
    private readonly CancellationTokenSource _cts = CancellationTokenSource.CreateLinkedTokenSource(cancelToken);
    private readonly object _resetLock = new();

    public async Task Reset()
    {
        IsCurrentTaskCompleted = false;

        CancellationTokenSource? capturedCts;
        Task? runningToAwait;

        lock (_resetLock)
        {
            try { _currentDelayCts?.Cancel(); }
            catch (ObjectDisposedException) { }

            _currentDelayCts = CancellationTokenSource.CreateLinkedTokenSource(_cts.Token);
            capturedCts = _currentDelayCts;

            runningToAwait = _currentTask;

            _worker = Task.Run(async () =>
            {
                try { await Task.Delay(Delay, capturedCts.Token).ConfigureAwait(false); }
                catch (OperationCanceledException) { return; }
                finally
                {
                    capturedCts.Dispose();
                }

                if (
                    !ReferenceEquals(_currentDelayCts, capturedCts) ||
                    capturedCts.IsCancellationRequested ||
                    _cts.IsCancellationRequested
                ) return;

                if (_currentTask is not null) await _currentTask.ConfigureAwait(false);

                var task = ScheduledTask();
                lock (_resetLock) _currentTask = task;

                try
                {
                    await task.ConfigureAwait(false);
                }
                finally
                {
                    lock (_resetLock)
                    {
                        _currentTask = null;
                        IsCurrentTaskCompleted = true;
                    }
                }
            }, _cts.Token);
        }

        if (runningToAwait is not null) await runningToAwait.ConfigureAwait(false);
    }

    public void Dispose()
    {
        _cts.Cancel();
        try { _worker?.Wait(); } catch { }
        _currentDelayCts?.Dispose();
        _cts.Dispose();
        GC.SuppressFinalize(this);
    }
}