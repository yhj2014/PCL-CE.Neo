using System.Collections.Generic;
using System.Linq;

namespace PCL_CE.Neo.Core.Utils.Diff;

public static class ItemDiff
{
    public static ItemDiffResult<T> Compute<T>(IEnumerable<T> oldItems, IEnumerable<T> newItems, IEqualityComparer<T>? comparer = null)
    {
        comparer ??= EqualityComparer<T>.Default;
        
        var oldSet = new HashSet<T>(oldItems, comparer);
        var newSet = new HashSet<T>(newItems, comparer);

        var result = new ItemDiffResult<T>();
        result.Added.AddRange(newItems.Where(item => !oldSet.Contains(item)));
        result.Removed.AddRange(oldItems.Where(item => !newSet.Contains(item)));
        result.Unchanged.AddRange(oldItems.Where(item => newSet.Contains(item)));

        return result;
    }

    public static ItemDiffResult<T> Compute<T>(IEnumerable<T> oldItems, IEnumerable<T> newItems, Func<T, T, bool> compareFunc)
    {
        var oldList = oldItems.ToList();
        var newList = newItems.ToList();

        var result = new ItemDiffResult<T>();
        result.Added.AddRange(newList.Where(newItem => !oldList.Any(oldItem => compareFunc(oldItem, newItem))));
        result.Removed.AddRange(oldList.Where(oldItem => !newList.Any(newItem => compareFunc(oldItem, newItem))));
        result.Unchanged.AddRange(oldList.Where(oldItem => newList.Any(newItem => compareFunc(oldItem, newItem))));

        return result;
    }

    public static ItemDiffResult<T> ComputeByKey<T, TKey>(IEnumerable<T> oldItems, IEnumerable<T> newItems, Func<T, TKey> keySelector)
    {
        var oldDict = oldItems.ToDictionary(keySelector);
        var newDict = newItems.ToDictionary(keySelector);

        var result = new ItemDiffResult<T>();
        result.Added.AddRange(newDict.Where(kvp => !oldDict.ContainsKey(kvp.Key)).Select(kvp => kvp.Value));
        result.Removed.AddRange(oldDict.Where(kvp => !newDict.ContainsKey(kvp.Key)).Select(kvp => kvp.Value));
        result.Unchanged.AddRange(oldDict.Where(kvp => newDict.ContainsKey(kvp.Key)).Select(kvp => kvp.Value));

        return result;
    }
}