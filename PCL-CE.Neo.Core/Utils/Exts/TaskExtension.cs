namespace PCL_CE.Neo.Core.Utils.Exts;

public static class TaskExtension
{
    public static async Task<T> WithTimeout<T>(this Task<T> task, TimeSpan timeout)
    {
        var timeoutTask = Task.Delay(timeout);
        var completedTask = await Task.WhenAny(task, timeoutTask);

        if (completedTask == timeoutTask)
            throw new TimeoutException($"操作超时，超过 {timeout.TotalMilliseconds}ms");

        return await task;
    }

    public static async Task WithTimeout(this Task task, TimeSpan timeout)
    {
        var timeoutTask = Task.Delay(timeout);
        var completedTask = await Task.WhenAny(task, timeoutTask);

        if (completedTask == timeoutTask)
            throw new TimeoutException($"操作超时，超过 {timeout.TotalMilliseconds}ms");

        await task;
    }

    public static async Task<T> WithCancellation<T>(this Task<T> task, CancellationToken cancellationToken)
    {
        var tcs = new TaskCompletionSource<T>();
        using (cancellationToken.Register(() => tcs.TrySetCanceled(cancellationToken)))
        {
            var completedTask = await Task.WhenAny(task, tcs.Task);
            return await completedTask;
        }
    }

    public static async Task WithCancellation(this Task task, CancellationToken cancellationToken)
    {
        var tcs = new TaskCompletionSource<bool>();
        using (cancellationToken.Register(() => tcs.TrySetCanceled(cancellationToken)))
        {
            var completedTask = await Task.WhenAny(task, tcs.Task);
            await completedTask;
        }
    }

    public static Task<T> AsTask<T>(this T value)
    {
        return Task.FromResult(value);
    }

    public static Task<T> AsTask<T>(this Func<T> func)
    {
        return Task.Run(func);
    }

    public static Task AsTask(this Action action)
    {
        return Task.Run(action);
    }

    public static async Task<TResult> ContinueWith<T, TResult>(this Task<T> task, Func<T, TResult> continuationFunction)
    {
        var result = await task;
        return continuationFunction(result);
    }

    public static async Task<TResult> ContinueWith<T, TResult>(this Task<T> task, Func<T, Task<TResult>> continuationFunction)
    {
        var result = await task;
        return await continuationFunction(result);
    }

    public static async Task<T> RetryAsync<T>(this Func<Task<T>> taskFactory, int maxRetries, TimeSpan delayBetweenRetries)
    {
        var lastException = default(Exception);

        for (var i = 0; i < maxRetries; i++)
        {
            try
            {
                return await taskFactory();
            }
            catch (Exception ex)
            {
                lastException = ex;
                if (i < maxRetries - 1)
                    await Task.Delay(delayBetweenRetries);
            }
        }

        throw lastException ?? new InvalidOperationException("重试失败");
    }

    public static async Task RetryAsync(this Func<Task> taskFactory, int maxRetries, TimeSpan delayBetweenRetries)
    {
        var lastException = default(Exception);

        for (var i = 0; i < maxRetries; i++)
        {
            try
            {
                await taskFactory();
                return;
            }
            catch (Exception ex)
            {
                lastException = ex;
                if (i < maxRetries - 1)
                    await Task.Delay(delayBetweenRetries);
            }
        }

        throw lastException ?? new InvalidOperationException("重试失败");
    }

    public static Task<T> DelayResult<T>(this T value, TimeSpan delay)
    {
        return Task.Delay(delay).ContinueWith(_ => value);
    }

    public static async Task<T[]> WhenAll<T>(this IEnumerable<Task<T>> tasks)
    {
        return await Task.WhenAll(tasks);
    }

    public static async Task WhenAll(this IEnumerable<Task> tasks)
    {
        await Task.WhenAll(tasks);
    }

    public static async Task<T> FirstCompleted<T>(this IEnumerable<Task<T>> tasks)
    {
        return await await Task.WhenAny(tasks);
    }

    public static async Task<T> FirstSuccessful<T>(this IEnumerable<Task<T>> tasks)
    {
        var remainingTasks = tasks.ToList();

        while (remainingTasks.Count > 0)
        {
            var completedTask = await Task.WhenAny(remainingTasks);
            remainingTasks.Remove(completedTask);

            try
            {
                return await completedTask;
            }
            catch
            {
            }
        }

        throw new AggregateException("所有任务都失败了");
    }
}