using System;

namespace PCL_CE.Neo.Core.Utils.VersionControl;

public struct FileVersionObjects
{
    public string Path { get; set; }
    public string Hash { get; set; }
    public ObjectType ObjectType { get; set; }
    public long Length { get; set; }
    public DateTime CreationTime { get; set; }
    public DateTime LastWriteTime { get; set; }
}