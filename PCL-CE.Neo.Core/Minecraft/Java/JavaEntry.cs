using System.Text.Json.Serialization;

namespace PCL_CE.Neo.Core.Minecraft.Java;

public class JavaEntry
{
    [JsonPropertyName("path")]
    public string? Path { get; set; }

    [JsonPropertyName("version")]
    public string? Version { get; set; }

    [JsonPropertyName("brand")]
    public JavaBrandType Brand { get; set; }

    [JsonPropertyName("architecture")]
    public string? Architecture { get; set; }

    [JsonPropertyName("is64Bit")]
    public bool Is64Bit { get; set; }

    [JsonPropertyName("isRecommended")]
    public bool IsRecommended { get; set; }

    [JsonPropertyName("priority")]
    public int Priority { get; set; }

    public JavaEntry()
    {
    }

    public JavaEntry(string path, string version, JavaBrandType brand, bool is64Bit)
    {
        Path = path;
        Version = version;
        Brand = brand;
        Is64Bit = is64Bit;
    }

    public override string ToString()
    {
        return $"{Brand} {Version} ({(Is64Bit ? "64-bit" : "32-bit")}) - {Path}";
    }

    public override bool Equals(object? obj)
    {
        if (obj is JavaEntry other)
        {
            return string.Equals(Path, other.Path, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(Version, other.Version, StringComparison.OrdinalIgnoreCase) &&
                   Brand == other.Brand &&
                   Is64Bit == other.Is64Bit;
        }

        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Path?.ToLowerInvariant(), Version?.ToLowerInvariant(), Brand, Is64Bit);
    }
}