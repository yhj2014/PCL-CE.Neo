using System;
using System.Collections.Generic;
using System.Linq;

namespace PCL_CE.Neo.Core.Utils;

public static class CollectionHelper
{
    public static bool IsNullOrEmpty<T>(IEnumerable<T>? collection)
    {
        return collection == null || !collection.Any();
    }

    public static int Count<T>(IEnumerable<T>? collection)
    {
        return collection?.Count() ?? 0;
    }

    public static List<T> ToListSafe<T>(IEnumerable<T>? collection)
    {
        return collection?.ToList() ?? new List<T>();
    }

    public static T[] ToArraySafe<T>(IEnumerable<T>? collection)
    {
        return collection?.ToArray() ?? Array.Empty<T>();
    }

    public static IEnumerable<T> DistinctBy<T, TKey>(IEnumerable<T> source, Func<T, TKey> keySelector)
    {
        var seenKeys = new HashSet<TKey>();
        
        foreach (var item in source)
        {
            var key = keySelector(item);
            if (seenKeys.Add(key))
                yield return item;
        }
    }

    public static IEnumerable<T> Flatten<T>(IEnumerable<IEnumerable<T>> source)
    {
        return source.SelectMany(x => x);
    }

    public static IEnumerable<T> Flatten<T>(IEnumerable<T> source, Func<T, IEnumerable<T>> selector)
    {
        foreach (var item in source)
        {
            yield return item;
            
            var children = selector(item);
            if (children != null)
            {
                foreach (var child in Flatten(children, selector))
                    yield return child;
            }
        }
    }

    public static IDictionary<TKey, TValue> Merge<TKey, TValue>(
        IDictionary<TKey, TValue> dict1, 
        IDictionary<TKey, TValue> dict2,
        Func<TValue, TValue, TValue> mergeFunc)
    {
        var result = new Dictionary<TKey, TValue>(dict1);
        
        foreach (var pair in dict2)
        {
            if (result.TryGetValue(pair.Key, out var existing))
                result[pair.Key] = mergeFunc(existing, pair.Value);
            else
                result[pair.Key] = pair.Value;
        }
        
        return result;
    }

    public static void ForEach<T>(IEnumerable<T> source, Action<T> action)
    {
        foreach (var item in source)
            action(item);
    }

    public static void ForEach<T>(IEnumerable<T> source, Action<T, int> action)
    {
        int index = 0;
        foreach (var item in source)
            action(item, index++);
    }

    public static IEnumerable<T> Shuffle<T>(IEnumerable<T> source)
    {
        var array = source.ToArray();
        
        for (int i = array.Length - 1; i > 0; i--)
        {
            int j = RandomUtils.Random.Next(i + 1);
            (array[j], array[i]) = (array[i], array[j]);
        }
        
        return array;
    }

    public static T? MaxBy<T, TKey>(IEnumerable<T> source, Func<T, TKey> keySelector) 
        where TKey : IComparable<TKey>
    {
        return source.OrderByDescending(keySelector).FirstOrDefault();
    }

    public static T? MinBy<T, TKey>(IEnumerable<T> source, Func<T, TKey> keySelector) 
        where TKey : IComparable<TKey>
    {
        return source.OrderBy(keySelector).FirstOrDefault();
    }

    public static IEnumerable<T> TakeRandom<T>(IEnumerable<T> source, int count)
    {
        return Shuffle(source).Take(count);
    }

    public static IEnumerable<T> SkipLast<T>(IEnumerable<T> source, int count)
    {
        var list = source.ToList();
        return list.Take(list.Count - count);
    }

    public static IEnumerable<T> TakeLast<T>(IEnumerable<T> source, int count)
    {
        var list = source.ToList();
        return list.Skip(Math.Max(0, list.Count - count));
    }

    public static IEnumerable<T> IntersectAll<T>(params IEnumerable<T>[] collections)
    {
        if (collections == null || collections.Length == 0)
            return Enumerable.Empty<T>();
        
        return collections.Aggregate((current, next) => current.Intersect(next));
    }

    public static IEnumerable<T> UnionAll<T>(params IEnumerable<T>[] collections)
    {
        if (collections == null || collections.Length == 0)
            return Enumerable.Empty<T>();
        
        return collections.Aggregate((current, next) => current.Union(next));
    }

    public static bool ContainsAll<T>(IEnumerable<T> source, IEnumerable<T> items)
    {
        var sourceSet = new HashSet<T>(source);
        return items.All(sourceSet.Contains);
    }

    public static bool ContainsAny<T>(IEnumerable<T> source, IEnumerable<T> items)
    {
        var sourceSet = new HashSet<T>(source);
        return items.Any(sourceSet.Contains);
    }

    public static IEnumerable<T> RemoveDuplicates<T>(IEnumerable<T> source)
    {
        return source.Distinct();
    }

    public static IDictionary<TKey, List<TValue>> GroupByToDictionary<TValue, TKey>(
        IEnumerable<TValue> source,
        Func<TValue, TKey> keySelector)
    {
        return source.GroupBy(keySelector)
                     .ToDictionary(g => g.Key, g => g.ToList());
    }

    public static IEnumerable<T> Page<T>(IEnumerable<T> source, int pageNumber, int pageSize)
    {
        return source.Skip((pageNumber - 1) * pageSize).Take(pageSize);
    }

    public static int GetPageCount<T>(IEnumerable<T> source, int pageSize)
    {
        var count = source.Count();
        return (int)Math.Ceiling((double)count / pageSize);
    }
}