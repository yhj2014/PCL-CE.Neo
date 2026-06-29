using System.Collections.Generic;

namespace PCL_CE.Neo.Core.Utils.Diff;

public class ItemDiffResult<T>
{
    public List<T> Added { get; }
    public List<T> Removed { get; }
    public List<T> Modified { get; }
    public List<T> Unchanged { get; }

    public int TotalChanges => Added.Count + Removed.Count + Modified.Count;

    public ItemDiffResult(List<T> added, List<T> removed, List<T> modified, List<T> unchanged)
    {
        Added = added ?? new List<T>();
        Removed = removed ?? new List<T>();
        Modified = modified ?? new List<T>();
        Unchanged = unchanged ?? new List<T>();
    }

    public bool HasChanges => TotalChanges > 0;

    public override string ToString()
    {
        return $"Added: {Added.Count}, Removed: {Removed.Count}, Modified: {Modified.Count}, Unchanged: {Unchanged.Count}";
    }
}