namespace PCL_CE.Neo.Core.Utils.Diff;

public static class ItemDiff
{
    public static ItemDiffResult<T> Compare<T>(IEnumerable<T> oldItems, IEnumerable<T> newItems)
    {
        return Compare(oldItems, newItems, EqualityComparer<T>.Default);
    }

    public static ItemDiffResult<T> Compare<T>(IEnumerable<T> oldItems, IEnumerable<T> newItems, IEqualityComparer<T> comparer)
    {
        var oldSet = new HashSet<T>(oldItems, comparer);
        var newSet = new HashSet<T>(newItems, comparer);

        var result = new ItemDiffResult<T>
        {
            Added = newSet.Where(item => !oldSet.Contains(item)).ToList(),
            Removed = oldSet.Where(item => !newSet.Contains(item)).ToList(),
            Unchanged = oldSet.Intersect(newSet, comparer).ToList()
        };

        return result;
    }

    public static ItemDiffResult<T> Compare<T>(IEnumerable<T> oldItems, IEnumerable<T> newItems, Func<T, T, bool> equals)
    {
        var oldList = oldItems.ToList();
        var newList = newItems.ToList();

        var result = new ItemDiffResult<T>();

        foreach (var oldItem in oldList)
        {
            if (!newList.Any(newItem => equals(oldItem, newItem)))
                result.Removed.Add(oldItem);
            else
                result.Unchanged.Add(oldItem);
        }

        foreach (var newItem in newList)
        {
            if (!oldList.Any(oldItem => equals(oldItem, newItem)))
                result.Added.Add(newItem);
        }

        return result;
    }

    public static ItemDiffResult<T> CompareWithModified<T>(IEnumerable<T> oldItems, IEnumerable<T> newItems, Func<T, T, bool> equals, Func<T, T, bool> isModified)
    {
        var oldList = oldItems.ToList();
        var newList = newItems.ToList();

        var result = new ItemDiffResult<T>();

        foreach (var oldItem in oldList)
        {
            var matchingNew = newList.FirstOrDefault(newItem => equals(oldItem, newItem));
            if (matchingNew == null)
                result.Removed.Add(oldItem);
            else if (isModified(oldItem, matchingNew))
                result.Modified.Add(matchingNew);
            else
                result.Unchanged.Add(oldItem);
        }

        foreach (var newItem in newList)
        {
            if (!oldList.Any(oldItem => equals(oldItem, newItem)))
                result.Added.Add(newItem);
        }

        return result;
    }

    public static ItemDiffResult<T> CompareById<T, TKey>(IEnumerable<T> oldItems, IEnumerable<T> newItems, Func<T, TKey> keySelector)
    {
        var oldDict = oldItems.ToDictionary(keySelector);
        var newDict = newItems.ToDictionary(keySelector);

        var result = new ItemDiffResult<T>
        {
            Added = newDict.Where(kvp => !oldDict.ContainsKey(kvp.Key)).Select(kvp => kvp.Value).ToList(),
            Removed = oldDict.Where(kvp => !newDict.ContainsKey(kvp.Key)).Select(kvp => kvp.Value).ToList(),
            Unchanged = oldDict.Where(kvp => newDict.ContainsKey(kvp.Key)).Select(kvp => kvp.Value).ToList()
        };

        return result;
    }

    public static ItemDiffResult<T> CompareByIdWithModified<T, TKey>(IEnumerable<T> oldItems, IEnumerable<T> newItems, Func<T, TKey> keySelector, Func<T, T, bool> isModified)
    {
        var oldDict = oldItems.ToDictionary(keySelector);
        var newDict = newItems.ToDictionary(keySelector);

        var result = new ItemDiffResult<T>();

        foreach (var (key, oldItem) in oldDict)
        {
            if (!newDict.ContainsKey(key))
                result.Removed.Add(oldItem);
            else if (isModified(oldItem, newDict[key]))
                result.Modified.Add(newDict[key]);
            else
                result.Unchanged.Add(oldItem);
        }

        foreach (var (key, newItem) in newDict)
        {
            if (!oldDict.ContainsKey(key))
                result.Added.Add(newItem);
        }

        return result;
    }

    public static List<T> GetAdded<T>(IEnumerable<T> oldItems, IEnumerable<T> newItems)
    {
        return Compare(oldItems, newItems).Added;
    }

    public static List<T> GetRemoved<T>(IEnumerable<T> oldItems, IEnumerable<T> newItems)
    {
        return Compare(oldItems, newItems).Removed;
    }

    public static List<T> GetModified<T>(IEnumerable<T> oldItems, IEnumerable<T> newItems, Func<T, T, bool> isModified)
    {
        return CompareWithModified(oldItems, newItems, EqualityComparer<T>.Default.Equals, isModified).Modified;
    }

    public static bool HasChanges<T>(IEnumerable<T> oldItems, IEnumerable<T> newItems)
    {
        return Compare(oldItems, newItems).HasChanges;
    }
}