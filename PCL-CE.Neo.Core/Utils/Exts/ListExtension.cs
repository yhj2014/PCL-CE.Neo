using System;
using System.Collections.Generic;

namespace PCL_CE.Neo.Core.Utils.Exts;

public static class ListExtension
{
    public static bool IsNullOrEmpty<T>(this List<T>? list) => list == null || list.Count == 0;

    public static void AddRangeIfNotNull<T>(this List<T> list, IEnumerable<T>? items)
    {
        if (items != null)
        {
            list.AddRange(items);
        }
    }

    public static void RemoveAll<T>(this List<T> list, Predicate<T> match) => list.RemoveAll(match);

    public static List<T> Shuffle<T>(this List<T> list)
    {
        var result = new List<T>(list);
        var random = new Random();
        for (int i = result.Count - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            (result[i], result[j]) = (result[j], result[i]);
        }
        return result;
    }

    public static T? FirstOrDefault<T>(this List<T> list, Predicate<T> match) =>
        list.Find(match);

    public static List<T> Where<T>(this List<T> list, Predicate<T> match) =>
        list.FindAll(match);

    public static List<TResult> Select<T, TResult>(this List<T> list, Func<T, TResult> selector)
    {
        var result = new List<TResult>();
        foreach (var item in list)
        {
            result.Add(selector(item));
        }
        return result;
    }

    public static bool Any<T>(this List<T> list, Predicate<T> match) =>
        list.Exists(match);

    public static bool All<T>(this List<T> list, Predicate<T> match) =>
        list.TrueForAll(match);
}