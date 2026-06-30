using System.Collections.Generic;

namespace PCL_CE.Neo.Core.Utils.Diff;

public class ItemDiffResult<T>
{
    public List<T> Added { get; set; } = new();
    public List<T> Removed { get; set; } = new();
    public List<System.Tuple<T, T>> Changed { get; set; } = new();
    public List<T> Unchanged { get; set; } = new();

    public int TotalChanges => Added.Count + Removed.Count + Changed.Count;

    public bool HasChanges => TotalChanges > 0;

    public override string ToString()
    {
        return $"Added: {Added.Count}, Removed: {Removed.Count}, Changed: {Changed.Count}, Unchanged: {Unchanged.Count}";
    }
}