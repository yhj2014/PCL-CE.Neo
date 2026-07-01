namespace PCL_CE.Neo.Core.Utils.Diff;

public class ItemDiffResult<T>
{
    public List<T> Added { get; set; } = new List<T>();
    public List<T> Removed { get; set; } = new List<T>();
    public List<T> Modified { get; set; } = new List<T>();
    public List<T> Unchanged { get; set; } = new List<T>();

    public bool HasChanges => Added.Count > 0 || Removed.Count > 0 || Modified.Count > 0;

    public int TotalChanges => Added.Count + Removed.Count + Modified.Count;

    public void Merge(ItemDiffResult<T> other)
    {
        Added.AddRange(other.Added);
        Removed.AddRange(other.Removed);
        Modified.AddRange(other.Modified);
        Unchanged.AddRange(other.Unchanged);
    }

    public override string ToString()
    {
        return $"Added: {Added.Count}, Removed: {Removed.Count}, Modified: {Modified.Count}, Unchanged: {Unchanged.Count}";
    }
}