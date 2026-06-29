using System;
using System.Collections.Generic;

namespace PCL_CE.Neo.Core.Utils.Diff;

public static class ItemDiff
{
    public static ItemDiffResult<T> Compare<T>(List<T> oldList, List<T> newList) where T : IEquatable<T>
    {
        try
        {
            var added = new List<T>();
            var removed = new List<T>();
            var modified = new List<T>();
            var unchanged = new List<T>();

            var oldSet = new HashSet<T>(oldList);
            var newSet = new HashSet<T>(newList);

            foreach (var item in newSet)
            {
                if (!oldSet.Contains(item))
                    added.Add(item);
                else
                    unchanged.Add(item);
            }

            foreach (var item in oldSet)
            {
                if (!newSet.Contains(item))
                    removed.Add(item);
            }

            return new ItemDiffResult<T>(added, removed, modified, unchanged);
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, "Item diff comparison failed");
            throw;
        }
    }

    public static ItemDiffResult<T> Compare<T>(List<T> oldList, List<T> newList, Func<T, T, bool> comparer)
    {
        try
        {
            var added = new List<T>();
            var removed = new List<T>();
            var modified = new List<T>();
            var unchanged = new List<T>();

            foreach (var newItem in newList)
            {
                var found = false;
                foreach (var oldItem in oldList)
                {
                    if (comparer(oldItem, newItem))
                    {
                        found = true;
                        unchanged.Add(newItem);
                        break;
                    }
                }
                
                if (!found)
                    added.Add(newItem);
            }

            foreach (var oldItem in oldList)
            {
                var found = false;
                foreach (var newItem in newList)
                {
                    if (comparer(oldItem, newItem))
                    {
                        found = true;
                        break;
                    }
                }
                
                if (!found)
                    removed.Add(oldItem);
            }

            return new ItemDiffResult<T>(added, removed, modified, unchanged);
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, "Item diff comparison failed");
            throw;
        }
    }
  
    public static ItemDiffResult<T> CompareById<T, TKey>(
        List<T> oldList, 
        List<T> newList, 
        Func<T, TKey> keySelector) where TKey : IEquatable<TKey>
    {
        try
        {
            var added = new List<T>();
            var removed = new List<T>();
            var modified = new List<T>();
            var unchanged = new List<T>();

            var oldDict = oldList.ToDictionary(keySelector);
            var newDict = newList.ToDictionary(keySelector);

            foreach (var pair in newDict)
            {
                if (oldDict.TryGetValue(pair.Key, out var oldItem))
                {
                    if (EqualityComparer<T>.Default.Equals(oldItem, pair.Value))
                        unchanged.Add(pair.Value);
                    else
                        modified.Add(pair.Value);
                }
                else
                {
                    added.Add(pair.Value);
                }
            }

            foreach (var pair in oldDict)
            {
                if (!newDict.ContainsKey(pair.Key))
                    removed.Add(pair.Value);
            }

            return new ItemDiffResult<T>(added, removed, modified, unchanged);
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, "Item diff comparison by ID failed");
            throw;
        }
    }
}