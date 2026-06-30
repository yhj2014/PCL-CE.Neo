using System;
using System.IO;

namespace PCL_CE.Neo.Core.Minecraft.Java;

public class JavaEntry
{
    public string Path { get; }
    public string Version { get; }
    public string Vendor { get; }
    public string Architecture { get; }
    public string JavaHome { get; }
    public bool IsValid { get; }

    public JavaEntry(string path, string version, string vendor, string architecture, string javaHome)
    {
        Path = path ?? throw new ArgumentNullException(nameof(path));
        Version = version ?? throw new ArgumentNullException(nameof(version));
        Vendor = vendor ?? throw new ArgumentNullException(nameof(vendor));
        Architecture = architecture ?? throw new ArgumentNullException(nameof(architecture));
        JavaHome = javaHome ?? throw new ArgumentNullException(nameof(javaHome));
        IsValid = File.Exists(path);
    }

    public override string ToString()
    {
        return $"{Vendor} Java {Version} ({Architecture})";
    }

    public override bool Equals(object? obj)
    {
        if (obj is not JavaEntry other)
            return false;

        return Path == other.Path && Version == other.Version;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Path, Version);
    }
}