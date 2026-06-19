using System.Threading;
using System.Threading.Tasks;

namespace PCL_CE.Neo.Core.Utils.Exts;

public static class TaskExtension
{
    public static async Task<T> WithTimeout<T>(this Task<T> task, TimeSpan timeout)
    {
        using var cts = new CancellationTokenSource(timeout);
        var completedTask = await Task.WhenAny(task, Task.Delay(timeout, cts.Token)).ConfigureAwait(false);
        if (completedTask == task)
        {
            cts.Cancel();
            return await task.ConfigureAwait(false);
        }
        throw new TimeoutException($"Task timed out after {timeout.TotalMilliseconds}ms");
    }

    public static async Task WithTimeout(this Task task, TimeSpan timeout)
    {
        using var cts = new CancellationTokenSource(timeout);
        var completedTask = await Task.WhenAny(task, Task.Delay(timeout, cts.Token)).ConfigureAwait(false);
        if (completedTask == task)
        {
            cts.Cancel();
            await task.ConfigureAwait(false);
        }
        else
        {
            throw new TimeoutException($"Task timed out after {timeout.TotalMilliseconds}ms");
        }
    }

    public static Task<T> AsTask<T>(this ValueTask<T> valueTask)
    {
        return valueTask.AsTask();
    }

    public static async Task<TResult> TryCatch<T, TResult>(this Task<T> task, Func<T, TResult> onSuccess, Func<Exception, TResult> onError)
    {
        try
        {
            var result = await task.ConfigureAwait(false);
            return onSuccess(result);
        }
        catch (Exception ex)
        {
            return onError(ex);
        }
    }
}