using System.Collections.Generic;
using System.Threading.Tasks;

namespace PCL_CE.Neo.Core.Utils.Exts;

public static class AsyncEnumerableExtensions
{
    public static async Task<List<T>> ToListAsync<T>(this IAsyncEnumerable<T> source)
    {
        var list = new List<T>();
        await foreach (var item in source)
        {
            list.Add(item);
        }
        return list;
    }

    public static async Task<int> CountAsync<T>(this IAsyncEnumerable<T> source)
    {
        int count = 0;
        await foreach (var _ in source)
        {
            count++;
        }
        return count;
    }

    public static async Task<T?> FirstOrDefaultAsync<T>(this IAsyncEnumerable<T> source)
    {
        await foreach (var item in source)
        {
            return item;
        }
        return default;
    }

    public static async Task<T?> FirstOrDefaultAsync<T>(this IAsyncEnumerable<T> source, Func<T, bool> predicate)
    {
        await foreach (var item in source)
        {
            if (predicate(item))
                return item;
        }
        return default;
    }

    public static async Task<bool> AnyAsync<T>(this IAsyncEnumerable<T> source)
    {
        await foreach (var _ in source)
        {
            return true;
        }
        return false;
    }

    public static async Task<bool> AnyAsync<T>(this IAsyncEnumerable<T> source, Func<T, bool> predicate)
    {
        await foreach (var item in source)
        {
            if (predicate(item))
                return true;
        }
        return false;
    }

    public static async Task<T> AggregateAsync<T>(this IAsyncEnumerable<T> source, Func<T, T, T> func)
    {
        bool first = true;
        T accumulator = default!;
        await foreach (var item in source)
        {
            if (first)
            {
                accumulator = item;
                first = false;
            }
            else
            {
                accumulator = func(accumulator, item);
            }
        }
        return accumulator;
    }

    public static async IAsyncEnumerable<TResult> SelectAsync<T, TResult>(this IAsyncEnumerable<T> source, Func<T, TResult> selector)
    {
        await foreach (var item in source)
        {
            yield return selector(item);
        }
    }

    public static async IAsyncEnumerable<T> WhereAsync<T>(this IAsyncEnumerable<T> source, Func<T, bool> predicate)
    {
        await foreach (var item in source)
        {
            if (predicate(item))
                yield return item;
        }
    }
}