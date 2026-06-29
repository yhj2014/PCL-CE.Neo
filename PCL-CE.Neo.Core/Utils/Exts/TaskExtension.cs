using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PCL.CE.Neo.Core.Utils.Exts;

public static class TaskExtensions
{
    public static async Task<Task<T>> WhenAnySuccess<T>(this IEnumerable<Task<T>> tasks)
    {
        ArgumentNullException.ThrowIfNull(tasks, nameof(tasks));
        var taskList = tasks.ToList();
        if (taskList.Count == 0)
            throw new ArgumentException("Task collection is empty.", nameof(tasks));

        var remaining = new HashSet<Task<T>>(taskList);

        while (remaining.Count > 0)
        {
            var completed = await Task.WhenAny(remaining);
            remaining.Remove(completed);
            if (completed.IsCompletedSuccessfully)
            {
                return completed;
            }
        }

        var faultedExceptions = taskList
            .Where(t => t.IsFaulted)
            .SelectMany(t => t.Exception?.Flatten().InnerExceptions ?? Enumerable.Empty<Exception>())
            .ToList();

        if (faultedExceptions.Count > 0)
        {
            throw new AggregateException("All connection attempts failed.", faultedExceptions);
        }

        if (taskList.Any(t => t.IsCanceled))
        {
            throw new OperationCanceledException("All connection attempts were canceled.");
        }

        throw new InvalidOperationException("All tasks completed but none succeeded, failed, or canceled.");
    }

    public static void Forget(this Task task)
    {
        _ = task.ContinueWith(t => _ = t.Exception, TaskContinuationOptions.OnlyOnFaulted);
    }

    public static async Task<T> WithTimeout<T>(this Task<T> task, TimeSpan timeout)
    {
        var completedTask = await Task.WhenAny(task, Task.Delay(timeout));
        if (completedTask == task)
        {
            return await task;
        }
        throw new TimeoutException($"Operation timed out after {timeout.TotalMilliseconds}ms");
    }

    public static async Task WithTimeout(this Task task, TimeSpan timeout)
    {
        var completedTask = await Task.WhenAny(task, Task.Delay(timeout));
        if (completedTask != task)
        {
            throw new TimeoutException($"Operation timed out after {timeout.TotalMilliseconds}ms");
        }
        await task;
    }
}