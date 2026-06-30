using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PCL_CE.Neo.Core.Utils.Exts;

public static class AsyncEnumerableExtensions
{
    extension<T>(IEnumerable<T> source)
    {
        public async Task ForEachAsync(
            Func<T, Task> action,
            int maxDegreeOfParallelism = 10)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(action);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxDegreeOfParallelism);

            var semaphore = new SemaphoreSlim(maxDegreeOfParallelism);

            var tasks = source.Select(async item =>
            {
                await semaphore.WaitAsync();
                try
                {
                    await action(item);
                }
                finally
                {
                    semaphore.Release();
                }
            });

            await Task.WhenAll(tasks);
        }

        public async Task ForEachAsync(
            Func<T, CancellationToken, Task> action,
            int maxDegreeOfParallelism = 10,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(action);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxDegreeOfParallelism);

            var semaphore = new SemaphoreSlim(maxDegreeOfParallelism);

            var tasks = source.Select(async item =>
            {
                await semaphore.WaitAsync(cancellationToken);
                try
                {
                    await action(item, cancellationToken);
                }
                finally
                {
                    semaphore.Release();
                }
            });

            await Task.WhenAll(tasks);
        }

        public async Task<IEnumerable<TResult>> SelectAsync<TResult>(
            Func<T, Task<TResult>> selector,
            int maxDegreeOfParallelism = 10,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(selector);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxDegreeOfParallelism);

            var semaphore = new SemaphoreSlim(maxDegreeOfParallelism);

            var tasks = source.Select(async item =>
            {
                await semaphore.WaitAsync(cancellationToken);
                try
                {
                    return await selector(item);
                }
                finally
                {
                    semaphore.Release();
                }
            });

            return await Task.WhenAll(tasks);
        }
    }
}