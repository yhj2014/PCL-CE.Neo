using System;
using System.IO;

namespace PCL_CE.Neo.Core.Minecraft.Java;

/// <summary>
/// Detailed Java installation information parsed from executable.
/// </summary>
public sealed class JavaInstallation
{
    /// <summary>
    /// Path to the Java folder (bin directory parent).
    /// </summary>
    public required string JavaFolder { get; init; }

    /// <summary>
    /// Path to the Java executable (java.exe or java).
    /// </summary>
    public string JavaExePath { get; init; } = string.Empty;

    /// <summary>
    /// Java version number.
    /// </summary>
    public required Version Version { get; init; }

    /// <summary>
    /// Java distribution brand.
    /// </summary>
    public JavaBrandType Brand { get; init; } = JavaBrandType.Unknown;

    /// <summary>
    /// PE header machine type (architecture identifier).
    /// </summary>
    public ushort MachineType { get; init; }

    /// <summary>
    /// Whether this is a 64-bit Java installation.
    /// </summary>
    public bool Is64Bit { get; init; }

    /// <summary>
    /// Whether this is a JRE (not JDK).
    /// </summary>
    public bool IsJre { get; init; }

    /// <summary>
    /// Major version number for quick comparison.
    /// </summary>
    public int MajorVersion => Version.Major == 1 ? Version.Minor : Version.Major;

    /// <summary>
    /// Check if the Java executable is still available on disk.
    /// </summary>
    public bool IsStillAvailable => File.Exists(JavaExePath);

    /// <summary>
    /// Get path to javac executable (for JDK detection).
    /// </summary>
    public string? JavacPath
    {
        get
        {
            if (IsJre) return null;
            var javac = Path.Combine(JavaFolder, "bin", OperatingSystem.IsWindows() ? "javac.exe" : "javac");
            return File.Exists(javac) ? javac : null;
        }
    }

    /// <summary>
    /// Get path to javaw executable (Windows only).
    /// </summary>
    public string? JavawPath
    {
        get
        {
            if (!OperatingSystem.IsWindows()) return null;
            var javaw = Path.Combine(JavaFolder, "bin", "javaw.exe");
            return File.Exists(javaw) ? javaw : null;
        }
    }

    /// <summary>
    /// Check if this Java can run the specified Minecraft version.
    /// </summary>
    public bool CanRunMcVersion(string mcVersion, int minMemoryMB = 0)
    {
        if (!IsStillAvailable) return false;
        if (!Is64Bit && minMemoryMB > 1500) return false;

        var requiredVersion = GetRequiredJavaVersion(mcVersion);
        return MajorVersion >= requiredVersion;
    }

    private static int GetRequiredJavaVersion(string mcVersion)
    {
        foreach (var kvp in JavaConsts.MinJavaVersionByMcVersion)
        {
            if (mcVersion.StartsWith(kvp.Key, StringComparison.OrdinalIgnoreCase))
                return kvp.Value;
        }
        return 8;
    }

    public override string ToString()
    {
        var type = IsJre ? "JRE" : "JDK";
        var arch = Is64Bit ? "64-bit" : "32-bit";
        return $"Java {Version} ({type}, {arch}, {Brand})";
    }
}