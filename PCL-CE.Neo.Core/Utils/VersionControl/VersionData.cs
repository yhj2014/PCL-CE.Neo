namespace PCL_CE.Neo.Core.Utils.VersionControl;

public class VersionData
{
    public string SnapshotName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public List<FileVersionObjects> Files { get; set; } = new List<FileVersionObjects>();
    public string? Description { get; set; }
    public string? ParentSnapshot { get; set; }
}