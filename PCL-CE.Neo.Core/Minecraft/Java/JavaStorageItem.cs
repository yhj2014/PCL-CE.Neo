namespace PCL_CE.Neo.Core.Minecraft.Java;

public class JavaStorageItem
{
    public string Id { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public JavaBrandType Brand { get; set; }
    public bool Is64Bit { get; set; }
    public string Architecture { get; set; } = string.Empty;
    public JavaSource Source { get; set; }
    public DateTime LastModified { get; set; }
    public bool IsRecommended { get; set; }
    public int Priority { get; set; }

    public JavaStorageItem()
    {
    }

    public JavaStorageItem(string id, string path, string version, JavaBrandType brand, bool is64Bit)
    {
        Id = id;
        Path = path;
        Version = version;
        Brand = brand;
        Is64Bit = is64Bit;
    }

    public JavaInstallation ToInstallation()
    {
        return new JavaInstallation(Path, Version, Brand, Is64Bit, Architecture);
    }

    public override string ToString()
    {
        return $"{Brand} {Version} ({(Is64Bit ? "64-bit" : "32-bit")})";
    }

    public override bool Equals(object? obj)
    {
        if (obj is JavaStorageItem other)
        {
            return string.Equals(Id, other.Id, StringComparison.OrdinalIgnoreCase);
        }

        return false;
    }

    public override int GetHashCode()
    {
        return Id.ToLowerInvariant().GetHashCode();
    }
}