using System;

namespace PCL_CE.Neo.Core.Minecraft.Java;

/// <summary>
/// Represents a discovered Java installation entry with metadata.
/// </summary>
public sealed class JavaEntry
{
    /// <summary>
    /// Java installation details parsed from the executable.
    /// </summary>
    public required JavaInstallation Installation { get; init; }

    /// <summary>
    /// Whether this Java entry is enabled for use.
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// How this Java was discovered or added.
    /// </summary>
    public JavaSource Source { get; set; }

    /// <summary>
    /// User-defined custom name for this Java installation.
    /// </summary>
    public string? CustomName { get; set; }

    /// <summary>
    /// Display name for UI purposes.
    /// </summary>
    public string DisplayName
    {
        get
        {
            if (!string.IsNullOrEmpty(CustomName))
                return CustomName;

            var version = Installation.Version.ToString();
            var type = Installation.IsJre ? "JRE" : "JDK";
            var arch = Installation.Is64Bit ? "64-bit" : "32-bit";
            var brand = Installation.Brand != JavaBrandType.Unknown ? Installation.Brand.ToString() : "Unknown";

            return $"Java {version} ({type}, {arch}, {brand})";
        }
    }

    /// <summary>
    /// Check if this Java is suitable for the specified Minecraft version.
    /// </summary>
    public bool IsSuitableForMcVersion(string mcVersion)
    {
        if (!IsEnabled || !Installation.IsStillAvailable)
            return false;

        var requiredVersion = GetRequiredJavaVersion(mcVersion);
        var normalizedJavaVersion = JavaManager.NormalizeVersion(Installation.Version);

        return normalizedJavaVersion.Major >= requiredVersion;
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

    public override string ToString() => DisplayName;
}