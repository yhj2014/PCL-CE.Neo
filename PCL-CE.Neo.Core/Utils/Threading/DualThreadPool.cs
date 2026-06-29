using System;
using System.Threading;
using System.Threading.Tasks;

namespace PCL_CE.Neo.Core.Utils.Threading;

public class DualThreadPool
{
    public int MaxThread { get; }

    private readonly TaskFactory _ioFactory;
    private readonly TaskFactory _cpuFactory;
    private readonly CancellationTokenSource _cts = new();

    public DualThreadPool(int maxThread)
    {
        if (maxThread < 1) throw new ArgumentOutOfRangeException(nameof(maxThread));

        MaxThread = maxThread;

        var ioScheduler = new LimitedConcurrencyLevelTaskScheduler(maxThread);
        var cpuScheduler = new LimitedConcurrencyLevelTaskScheduler(maxThread);
        var cancellationToken = _cts.Token;

        _ioFactory = new TaskFactory(
            cancellationToken,
            TaskCreationOptions.DenyChildAttach,
            TaskContinuationOptions.None,
            ioScheduler);

        _cpuFactory = new TaskFactory(
            cancellationToken,
            TaskCreationOptions.DenyChildAttach,
            TaskContinuationOptions.None,
            cpuScheduler);
    }

    public Task QueueIo(Action work) => _ioFactory.StartNew(work);

    public Task QueueIo(Func<Task> work) => _ioFactory.StartNew(work).Unwrap();

    public Task QueueCpu(Action work) => _cpuFactory.StartNew(work);

    public Task QueueCpu(Func<Task> work) => _cpuFactory.StartNew(work).Unwrap();

    public void CancelAll() => _cts.Cancel();
}