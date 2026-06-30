using System;

namespace PCL_CE.Neo.Core.Utils.VersionControl;

public struct VersionData
{
    public string NodeId { get; set; }
    public DateTime Created { get; set; }
    public string Name { get; set; }
    public string Desc { get; set; }
    public long Version { get; set; }
}