using System;

namespace PCL_CE.Neo.Core.Utils.VersionControl;

public class VersionData : IVersionData
{
    public string Version { get; set; } = string.Empty;
    public DateTime ReleaseDate { get; set; } = DateTime.Now;
    public string? Changelog { get; set; }
    public string? DownloadUrl { get; set; }
    public string? Hash { get; set; }
    public bool IsPreRelease { get; set; }
    public bool IsStable { get; set; } = true;

    public string? NodeId { get; set; }
    public DateTime? Created { get; set; }
    public string? Name { get; set; }
    public string? Desc { get; set; }

    public override string ToString()
    {
        return $"Version {Version} ({ReleaseDate:yyyy-MM-dd})";
    }
}