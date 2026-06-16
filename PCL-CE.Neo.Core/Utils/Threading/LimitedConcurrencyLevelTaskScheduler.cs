using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PCL_CE.Neo.Core.Utils.Threading;

public class LimitedConcurrencyLevelTaskScheduler : TaskScheduler
{
    private readonly LinkedList<Task> _tasks = new();
    private readonly int _maxConcurrencyLevel;
    private int _delegatesQueuedOrRunning = 0;

    public LimitedConcurrencyLevelTaskScheduler(int maxConcurrencyLevel)
    {
        if (maxConcurrencyLevel < 1)
            throw new ArgumentOutOfRangeException(nameof(maxConcurrencyLevel));

        _maxConcurrencyLevel = maxConcurrencyLevel;
    }

    public override int MaximumConcurrencyLevel => _maxConcurrencyLevel;

    protected override IEnumerable<Task> GetScheduledTasks()
    {
        bool lockTaken = false;
        try
        {
            Monitor.TryEnter(_tasks, ref lockTaken);
            if (lockTaken)
                return _tasks.ToArray();
            else
                throw new NotSupportedException();
        }
        finally
        {
            if (lockTaken)
                Monitor.Exit(_tasks);
        }
    }

    protected override void QueueTask(Task task)
    {
        lock (_tasks)
        {
            _tasks.AddLast(task);
            if (_delegatesQueuedOrRunning < _maxConcurrencyLevel)
            {
                ++_delegatesQueuedOrRunning;
                NotifyThreadPoolOfPendingWork();
            }
        }
    }

    protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
    {
        if (_delegatesQueuedOrRunning >= _maxConcurrencyLevel)
            return false;

        if (taskWasPreviouslyQueued)
        {
            if (TryDequeue(task))
                --_delegatesQueuedOrRunning;
            else
                return false;
        }
        else
        {
            ++_delegatesQueuedOrRunning;
        }

        try
        {
            return base.TryExecuteTask(task);
        }
        finally
        {
            --_delegatesQueuedOrRunning;
            NotifyThreadPoolOfPendingWork();
        }
    }

    protected override bool TryDequeue(Task task)
    {
        lock (_tasks)
        {
            return _tasks.Remove(task);
        }
    }

    private void NotifyThreadPoolOfPendingWork()
    {
        ThreadPool.UnsafeQueueUserWorkItem(_ =>
        {
            while (true)
            {
                Task item;
                lock (_tasks)
                {
                    if (_tasks.Count == 0)
                    {
                        --_delegatesQueuedOrRunning;
                        break;
                    }

                    item = _tasks.First.Value;
                    _tasks.RemoveFirst();
                }

                base.TryExecuteTask(item);
            }
        }, null);
    }
}