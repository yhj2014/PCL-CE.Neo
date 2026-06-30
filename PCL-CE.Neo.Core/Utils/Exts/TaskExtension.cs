using System;
using System.Threading;
using System.Threading.Tasks;

namespace PCL_CE.Neo.Core.Utils.Exts;

public static class TaskExtension
{
    public static async Task<T> WithTimeout<T>(this Task<T> task, TimeSpan timeout, CancellationToken cancellationToken = default)
    {
        if (task == null)
            throw new ArgumentNullException(nameof(task));

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var timeoutTask = Task.Delay(timeout, cts.Token);

        var completedTask = await Task.WhenAny(task, timeoutTask);

        if (completedTask == timeoutTask)
        {
            throw new TimeoutException($"Operation timed out after {timeout.TotalMilliseconds}ms");
        }

        cts.Cancel();
        return await task;
    }

    public static async Task WithTimeout(this Task task, TimeSpan timeout, CancellationToken cancellationToken = default)
    {
        if (task == null)
            throw new ArgumentNullException(nameof(task));

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var timeoutTask = Task.Delay(timeout, cts.Token);

        var completedTask = await Task.WhenAny(task, timeoutTask);

        if (completedTask == timeoutTask)
        {
            throw new TimeoutException($"Operation timed out after {timeout.TotalMilliseconds}ms");
        }

        cts.Cancel();
        await task;
    }

    public static async Task<T> WithTimeout<T>(this Task<T> task, int millisecondsTimeout, CancellationToken cancellationToken = default)
    {
        return await task.WithTimeout(TimeSpan.FromMilliseconds(millisecondsTimeout), cancellationToken);
    }

    public static async Task WithTimeout(this Task task, int millisecondsTimeout, CancellationToken cancellationToken = default)
    {
        await task.WithTimeout(TimeSpan.FromMilliseconds(millisecondsTimeout), cancellationToken);
    }

    public static async Task<TResult> ContinueWithIgnoreCancellation<TResult>(this Task<TResult> task, Func<Task<TResult>, TResult> continuation)
    {
        if (task == null)
            throw new ArgumentNullException(nameof(task));
        if (continuation == null)
            throw new ArgumentNullException(nameof(continuation));

        try
        {
            await task;
            return continuation(task);
        }
        catch (OperationCanceledException)
        {
            return continuation(task);
        }
    }

    public static async Task ContinueWithIgnoreCancellation(this Task task, Action<Task> continuation)
    {
        if (task == null)
            throw new ArgumentNullException(nameof(task));
        if (continuation == null)
            throw new ArgumentNullException(nameof(continuation));

        try
        {
            await task;
            continuation(task);
        }
        catch (OperationCanceledException)
        {
            continuation(task);
        }
    }

    public static Task<T> FromResult<T>(T value)
    {
        return Task.FromResult(value);
    }

    public static Task FromException(Exception exception)
    {
        if (exception == null)
            throw new ArgumentNullException(nameof(exception));

        var tcs = new TaskCompletionSource<bool>();
        tcs.SetException(exception);
        return tcs.Task;
    }

    public static Task<T> FromException<T>(Exception exception)
    {
        if (exception == null)
            throw new ArgumentNullException(nameof(exception));

        var tcs = new TaskCompletionSource<T>();
        tcs.SetException(exception);
        return tcs.Task;
    }

    public static Task<T> FromCanceled<T>(CancellationToken cancellationToken)
    {
        var tcs = new TaskCompletionSource<T>();
        tcs.SetCanceled(cancellationToken);
        return tcs.Task;
    }
}