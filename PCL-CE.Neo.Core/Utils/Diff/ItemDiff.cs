using System;
using System.Collections.Generic;
using System.Linq;

namespace PCL_CE.Neo.Core.Utils.Diff;

public static class ItemDiff
{
    public static ItemDiffResult<T> Compare<T>(IEnumerable<T> oldItems, IEnumerable<T> newItems, Func<T, T, bool> comparer)
    {
        if (oldItems == null)
            throw new ArgumentNullException(nameof(oldItems));
        if (newItems == null)
            throw new ArgumentNullException(nameof(newItems));
        if (comparer == null)
            throw new ArgumentNullException(nameof(comparer));

        var oldList = oldItems.ToList();
        var newList = newItems.ToList();

        var added = new List<T>();
        var removed = new List<T>();
        var changed = new List<Tuple<T, T>>();
        var unchanged = new List<T>();

        foreach (var newItem in newList)
        {
            var matched = oldList.FirstOrDefault(oldItem => comparer(oldItem, newItem));
            if (matched == null)
            {
                added.Add(newItem);
            }
            else
            {
                changed.Add(Tuple.Create(matched, newItem));
            }
        }

        foreach (var oldItem in oldList)
        {
            var matched = newList.FirstOrDefault(newItem => comparer(newItem, oldItem));
            if (matched == null)
            {
                removed.Add(oldItem);
            }
        }

        return new ItemDiffResult<T>
        {
            Added = added,
            Removed = removed,
            Changed = changed,
            Unchanged = unchanged
        };
    }

    public static ItemDiffResult<T> Compare<T>(IEnumerable<T> oldItems, IEnumerable<T> newItems) where T : IEquatable<T>
    {
        return Compare(oldItems, newItems, (a, b) => a.Equals(b));
    }

    public static ItemDiffResult<T> CompareById<T>(IEnumerable<T> oldItems, IEnumerable<T> newItems, Func<T, string> idSelector)
    {
        if (idSelector == null)
            throw new ArgumentNullException(nameof(idSelector));

        return Compare(oldItems, newItems, (a, b) => idSelector(a) == idSelector(b));
    }
}