using System;
using System.Collections.Generic;

namespace PCL.CE.Neo.Core.Utils.Exts;

public static class ListExtension
{
    public static void AddRangeIfNotExists<T>(this List<T> list, IEnumerable<T> items)
    {
        foreach (var item in items)
        {
            if (!list.Contains(item))
            {
                list.Add(item);
            }
        }
    }

    public static void RemoveAll<T>(this List<T> list, Func<T, bool> predicate)
    {
        list.RemoveAll(item => predicate(item));
    }

    public static List<T> Shuffle<T>(this List<T> list)
    {
        var result = new List<T>(list);
        var rng = new Random();
        int n = result.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            (result[k], result[n]) = (result[n], result[k]);
        }
        return result;
    }

    public static List<T> DistinctBy<T, TKey>(this List<T> list, Func<T, TKey> keySelector)
    {
        var seen = new HashSet<TKey>();
        var result = new List<T>();
        foreach (var item in list)
        {
            var key = keySelector(item);
            if (seen.Add(key))
            {
                result.Add(item);
            }
        }
        return result;
    }

    public static int IndexOf<T>(this List<T> list, Func<T, bool> predicate)
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (predicate(list[i]))
            {
                return i;
            }
        }
        return -1;
    }

    public static T? FirstOrDefault<T>(this List<T> list, Func<T, bool> predicate)
    {
        foreach (var item in list)
        {
            if (predicate(item))
            {
                return item;
            }
        }
        return default;
    }

    public static T? LastOrDefault<T>(this List<T> list, Func<T, bool> predicate)
    {
        for (int i = list.Count - 1; i >= 0; i--)
        {
            if (predicate(list[i]))
            {
                return list[i];
            }
        }
        return default;
    }
}