using System.Collections.Generic;
using System.Threading.Tasks;

namespace PCL_CE.Neo.Core.Utils.Exts;

public static class AsyncEnumerableExtensions
{
    public static async Task<List<T>> ToListAsync<T>(this IAsyncEnumerable<T> source)
    {
        List<T> list = new List<T>();
        await foreach (T item in source)
            list.Add(item);
        return list;
    }

    public static async Task<T[]> ToArrayAsync<T>(this IAsyncEnumerable<T> source)
    {
        List<T> list = await source.ToListAsync();
        return list.ToArray();
    }

    public static async Task<int> CountAsync<T>(this IAsyncEnumerable<T> source)
    {
        int count = 0;
        await foreach (T _ in source)
            count++;
        return count;
    }

    public static async Task<bool> AnyAsync<T>(this IAsyncEnumerable<T> source)
    {
        await foreach (T _ in source)
            return true;
        return false;
    }

    public static async Task<bool> AnyAsync<T>(this IAsyncEnumerable<T> source, Func<T, bool> predicate)
    {
        await foreach (T item in source)
            if (predicate(item))
                return true;
        return false;
    }

    public static async Task<T?> FirstOrDefaultAsync<T>(this IAsyncEnumerable<T> source)
    {
        await foreach (T item in source)
            return item;
        return default;
    }

    public static async Task<T?> FirstOrDefaultAsync<T>(this IAsyncEnumerable<T> source, Func<T, bool> predicate)
    {
        await foreach (T item in source)
            if (predicate(item))
                return item;
        return default;
    }

    public static async Task<T?> SingleOrDefaultAsync<T>(this IAsyncEnumerable<T> source)
    {
        T? result = default;
        bool found = false;
        await foreach (T item in source)
        {
            if (found)
                return default;
            result = item;
            found = true;
        }
        return result;
    }

    public static async Task<T> SumAsync<T>(this IAsyncEnumerable<T> source, Func<T, long> selector)
    {
        long sum = 0;
        await foreach (T item in source)
            sum += selector(item);
        return default;
    }

    public static async Task<List<TResult>> SelectAsync<TSource, TResult>(this IAsyncEnumerable<TSource> source, Func<TSource, TResult> selector)
    {
        List<TResult> list = new List<TResult>();
        await foreach (TSource item in source)
            list.Add(selector(item));
        return list;
    }

    public static async Task<List<T>> WhereAsync<T>(this IAsyncEnumerable<T> source, Func<T, bool> predicate)
    {
        List<T> list = new List<T>();
        await foreach (T item in source)
            if (predicate(item))
                list.Add(item);
        return list;
    }
}