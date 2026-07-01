using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PCL_CE.Neo.Core.Utils.Threading;

public class LimitedConcurrencyLevelTaskScheduler : TaskScheduler
{
    private readonly object _lock = new object();
    private readonly int _maxConcurrencyLevel;
    private readonly ConcurrentQueue<Task> _tasks = new ConcurrentQueue<Task>();
    private int _currentConcurrencyLevel;

    public LimitedConcurrencyLevelTaskScheduler(int maxConcurrencyLevel)
    {
        if (maxConcurrencyLevel < 1)
            throw new ArgumentOutOfRangeException(nameof(maxConcurrencyLevel), "最大并发级别必须至少为1。");

        _maxConcurrencyLevel = maxConcurrencyLevel;
    }

    public override int MaximumConcurrencyLevel => _maxConcurrencyLevel;

    protected override IEnumerable<Task> GetScheduledTasks()
    {
        return _tasks.ToArray();
    }

    protected override void QueueTask(Task task)
    {
        _tasks.Enqueue(task);
        TryExecuteTask();
    }

    protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
    {
        if (_currentConcurrencyLevel >= _maxConcurrencyLevel)
            return false;

        if (!_tasks.TryDequeue(out _))
            return false;

        Interlocked.Increment(ref _currentConcurrencyLevel);
        try
        {
            return base.TryExecuteTask(task);
        }
        finally
        {
            Interlocked.Decrement(ref _currentConcurrencyLevel);
            TryExecuteTask();
        }
    }

    private void TryExecuteTask()
    {
        while (_currentConcurrencyLevel < _maxConcurrencyLevel && _tasks.TryDequeue(out var task))
        {
            Interlocked.Increment(ref _currentConcurrencyLevel);
            ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    base.TryExecuteTask(task);
                }
                finally
                {
                    Interlocked.Decrement(ref _currentConcurrencyLevel);
                    TryExecuteTask();
                }
            });
        }
    }
}