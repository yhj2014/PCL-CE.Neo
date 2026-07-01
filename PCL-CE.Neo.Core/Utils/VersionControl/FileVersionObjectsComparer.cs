using System.Collections.Generic;

namespace PCL_CE.Neo.Core.Utils.VersionControl;

public class FileVersionObjectsComparer : IEqualityComparer<FileVersionObjects>
{
    public bool Equals(FileVersionObjects? x, FileVersionObjects? y)
    {
        if (x == null && y == null)
            return true;
        if (x == null || y == null)
            return false;

        return x.Path == y.Path && x.Type == y.Type && x.Hash == y.Hash;
    }

    public int GetHashCode(FileVersionObjects obj)
    {
        return HashCode.Combine(obj.Path, obj.Type, obj.Hash);
    }
}