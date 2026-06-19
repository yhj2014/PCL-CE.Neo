using System.Collections.Generic;

namespace PCL_CE.Neo.Core.Utils.Exts;

public static class ListExtension
{
    public static void AddRange<T>(this List<T> list, params T[] items)
    {
        list.AddRange((IEnumerable<T>)items);
    }

    public static bool AddIfNotNull<T>(this List<T> list, T? item) where T : class
    {
        if (item != null)
        {
            list.Add(item);
            return true;
        }
        return false;
    }

    public static bool AddIfNotNull<T>(this List<T> list, T? item) where T : struct
    {
        if (item.HasValue)
        {
            list.Add(item.Value);
            return true;
        }
        return false;
    }

    public static T? FirstOrNull<T>(this List<T> list, Func<T, bool> predicate) where T : class
    {
        foreach (var item in list)
        {
            if (predicate(item))
                return item;
        }
        return null;
    }
}