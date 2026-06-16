using System;
using System.Threading;
using System.Threading.Tasks;

namespace PCL_CE.Neo.Core.Utils.Exts;

public static class TaskExtension
{
    public static async Task<T> WithTimeout<T>(this Task<T> task, TimeSpan timeout)
    {
        using var cts = new CancellationTokenSource(timeout);
        var completedTask = await Task.WhenAny(task, Task.Delay(timeout, cts.Token));
        
        if (completedTask == task)
        {
            return await task;
        }
        
        throw new TimeoutException("任务执行超时");
    }

    public static async Task WithTimeout(this Task task, TimeSpan timeout)
    {
        using var cts = new CancellationTokenSource(timeout);
        var completedTask = await Task.WhenAny(task, Task.Delay(timeout, cts.Token));
        
        if (completedTask != task)
        {
            throw new TimeoutException("任务执行超时");
        }
        
        await task;
    }

    public static async Task<T> WithCancellation<T>(this Task<T> task, CancellationToken cancellationToken)
    {
        var tcs = new TaskCompletionSource<bool>();
        using var registration = cancellationToken.Register(s => ((TaskCompletionSource<bool>)s!).TrySetResult(true), tcs);
        
        if (task != await Task.WhenAny(task, tcs.Task))
        {
            throw new OperationCanceledException(cancellationToken);
        }
        
        return await task;
    }

    public static async Task WithCancellation(this Task task, CancellationToken cancellationToken)
    {
        var tcs = new TaskCompletionSource<bool>();
        using var registration = cancellationToken.Register(s => ((TaskCompletionSource<bool>)s!).TrySetResult(true), tcs);
        
        if (task != await Task.WhenAny(task, tcs.Task))
        {
            throw new OperationCanceledException(cancellationToken);
        }
        
        await task;
    }

    public static Task<T> FromResult<T>(T result) => Task.FromResult(result);

    public static Task CompletedTask => Task.CompletedTask;
}