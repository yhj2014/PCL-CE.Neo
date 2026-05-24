using PCL_CE.Neo.Core.Abstractions;
using PCL_CE.Neo.Core.App;

namespace PCL_CE.Neo.Core.Adapters;

/// <summary>
/// Adapter for paths functionality
/// </summary>
public class PathsAdapter : IPathsAdapter
{
    public string Data => Paths.Data;
    public string SharedData => Paths.SharedData;
    public string SharedLocalData => Paths.SharedLocalData;
    public string Temp => Paths.Temp;
    public string OldSharedData => Paths.OldSharedData;

    public string GetSpecialPath(Environment.SpecialFolder folder, string relative)
    {
        return Paths.GetSpecialPath(folder, relative);
    }

    public void EnsureDirectories()
    {
        // Paths initialization already ensures directories exist
    }
}
