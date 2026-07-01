namespace PCL_CE.Neo.Core.Minecraft.Java;

public class JavaInstallation
{
    public string Path { get; }
    public string Version { get; }
    public JavaBrandType Brand { get; }
    public bool Is64Bit { get; }
    public string Architecture { get; }

    public JavaInstallation(string path, string version, JavaBrandType brand, bool is64Bit, string architecture = "")
    {
        Path = path ?? throw new ArgumentNullException(nameof(path));
        Version = version ?? throw new ArgumentNullException(nameof(version));
        Brand = brand;
        Is64Bit = is64Bit;
        Architecture = architecture ?? string.Empty;
    }

    public string GetJavaExecutablePath()
    {
        if (OperatingSystem.IsWindows())
            return System.IO.Path.Combine(Path, "bin", "java.exe");

        return System.IO.Path.Combine(Path, "bin", "java");
    }

    public string GetJavacExecutablePath()
    {
        if (OperatingSystem.IsWindows())
            return System.IO.Path.Combine(Path, "bin", "javac.exe");

        return System.IO.Path.Combine(Path, "bin", "javac");
    }

    public bool IsValid()
    {
        return System.IO.File.Exists(GetJavaExecutablePath());
    }

    public override string ToString()
    {
        return $"{Brand} {Version} ({(Is64Bit ? "64-bit" : "32-bit")})";
    }

    public override bool Equals(object? obj)
    {
        if (obj is JavaInstallation other)
        {
            return string.Equals(Path, other.Path, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(Version, other.Version, StringComparison.OrdinalIgnoreCase);
        }

        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Path?.ToLowerInvariant(), Version?.ToLowerInvariant());
    }
}