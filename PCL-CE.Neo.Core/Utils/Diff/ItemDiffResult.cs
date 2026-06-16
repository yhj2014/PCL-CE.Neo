using System.Collections.Generic;

namespace PCL_CE.Neo.Core.Utils.Diff;

public class ItemDiffResult<T>
{
    public List<T> Added { get; } = new();
    public List<T> Removed { get; } = new();
    public List<T> Unchanged { get; } = new();
}