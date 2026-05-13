namespace PCL_CE.Neo.Core.Models;

/// <summary>
/// Metadata model.
/// </summary>
public class MetadataModel
{
    /// <summary>
    /// Version information.
    /// </summary>
    public VersionModel Version { get; set; } = new();
}

/// <summary>
/// Version model.
/// </summary>
public class VersionModel
{
    /// <summary>
    /// Base name of the version.
    /// </summary>
    public string BaseName { get; set; } = string.Empty;
    
    /// <summary>
    /// Internal code of the version.
    /// </summary>
    public int Code { get; set; }
    
    /// <summary>
    /// Branch name of the version.
    /// </summary>
    public string BranchName { get; set; } = string.Empty;
}
