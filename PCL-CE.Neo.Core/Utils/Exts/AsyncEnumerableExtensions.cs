using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PCL.CE.Neo.Core.Utils.Exts;

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

    public static async Task<List<TResult>> SelectAsync<TSource, TResult>(
        this IEnumerable<TSource> source,
        Func<TSource, Task<TResult>> selector)
    {
        var results = new List<TResult>();
        foreach (var item in source)
        {
            results.Add(await selector(item));
        }
        return results;
    }

    public static async Task<List<T>> WhereAsync<T>(
        this IEnumerable<T> source,
        Func<T, Task<bool>> predicate)
    {
        var results = new List<T>();
        foreach (var item in source)
        {
            if (await predicate(item))
            {
                results.Add(item);
            }
        }
        return results;
    }

    public static async Task<T> FirstOrDefaultAsync<T>(
        this IEnumerable<T> source,
        Func<T, Task<bool>> predicate)
    {
        foreach (var item in source)
        {
            if (await predicate(item))
            {
                return item;
            }
        }
        return default!;
    }

    public static async Task<bool> AnyAsync<T>(
        this IEnumerable<T> source,
        Func<T, Task<bool>> predicate)
    {
        foreach (var item in source)
        {
            if (await predicate(item))
            {
                return true;
            }
        }
        return false;
    }
}