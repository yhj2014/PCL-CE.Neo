using System;
using System.Threading;
using System.Threading.Tasks;

namespace PCL_CE.Neo.Core.Utils.Threading;

public class LimitedTaskPool
{
    public int MaxThread { get; }

    private readonly TaskFactory _factory;
    private readonly CancellationTokenSource _cts = new();

    public LimitedTaskPool(int maxThread)
    {
        if (maxThread < 1) throw new ArgumentOutOfRangeException(nameof(maxThread));

        MaxThread = maxThread;

        var scheduler = new LimitedConcurrencyLevelTaskScheduler(maxThread);
        var cancellationToken = _cts.Token;

        _factory = new TaskFactory(
            cancellationToken,
            TaskCreationOptions.DenyChildAttach,
            TaskContinuationOptions.None,
            scheduler);
    }

    public Task Submit(Action work) => _factory.StartNew(work);

    public Task Submit(Func<Task> work) => _factory.StartNew(work).Unwrap();

    public void CancelAll() => _cts.Cancel();
}