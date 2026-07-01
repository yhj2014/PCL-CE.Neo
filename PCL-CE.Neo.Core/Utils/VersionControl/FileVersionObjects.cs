namespace PCL_CE.Neo.Core.Utils.VersionControl;

public class FileVersionObjects
{
    public string Path { get; set; } = string.Empty;
    public ObjectType Type { get; set; }
    public string? Hash { get; set; }
    public long Size { get; set; }
    public DateTime LastModified { get; set; }
    public DateTime Created { get; set; }
    public string? Content { get; set; }
    public List<string>? Children { get; set; }

    public override bool Equals(object? obj)
    {
        if (obj is not FileVersionObjects other)
            return false;

        return Path == other.Path && Type == other.Type && Hash == other.Hash;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Path, Type, Hash);
    }
}