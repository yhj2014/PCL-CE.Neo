using PCL_CE.Neo.Core.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace PCL_CE.Neo.Core.Utils;

public class SemVer : IComparable<SemVer>, IEquatable<SemVer>
{
    private const string ModuleName = "SemVer";
    private static readonly Regex VersionRegex = new(
        @"^(?<major>0|[1-9]\d*)\.(?<minor>0|[1-9]\d*)\.(?<patch>0|[1-9]\d*)(?:-(?<prerelease>(?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*)(?:\.(?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*))*))?(?:\+(?<buildmetadata>[0-9a-zA-Z-]+(?:\.[0-9a-zA-Z-]+)*))?$",
        RegexOptions.Compiled
    );

    public int Major { get; }
    public int Minor { get; }
    public int Patch { get; }
    public string? Prerelease { get; }
    public string? BuildMetadata { get; }

    public SemVer(int major, int minor, int patch, string? prerelease = null, string? buildMetadata = null)
    {
        if (major < 0) throw new ArgumentOutOfRangeException(nameof(major), "Major version cannot be negative");
        if (minor < 0) throw new ArgumentOutOfRangeException(nameof(minor), "Minor version cannot be negative");
        if (patch < 0) throw new ArgumentOutOfRangeException(nameof(patch), "Patch version cannot be negative");

        Major = major;
        Minor = minor;
        Patch = patch;
        Prerelease = prerelease;
        BuildMetadata = buildMetadata;
    }

    public static bool TryParse(string version, out SemVer? result)
    {
        result = null;
        
        try
        {
            result = Parse(version);
            return true;
        }
        catch (Exception ex)
        {
            LogWrapper.Debug($"Failed to parse version '{version}': {ex.Message}", ModuleName);
            return false;
        }
    }

    public static SemVer Parse(string version)
    {
        if (string.IsNullOrWhiteSpace(version))
            throw new ArgumentNullException(nameof(version), "Version string cannot be null or empty");

        var match = VersionRegex.Match(version.Trim());
        
        if (!match.Success)
            throw new FormatException($"Invalid semantic version format: {version}");

        var major = int.Parse(match.Groups["major"].Value);
        var minor = int.Parse(match.Groups["minor"].Value);
        var patch = int.Parse(match.Groups["patch"].Value);
        var prerelease = match.Groups["prerelease"].Success ? match.Groups["prerelease"].Value : null;
        var buildMetadata = match.Groups["buildmetadata"].Success ? match.Groups["buildmetadata"].Value : null;

        LogWrapper.Debug($"Parsed version: {major}.{minor}.{patch}{(prerelease != null ? $"-{prerelease}" : "")}", ModuleName);
        return new SemVer(major, minor, patch, prerelease, buildMetadata);
    }

    public int CompareTo(SemVer? other)
    {
        if (other is null) return 1;

        var majorCompare = Major.CompareTo(other.Major);
        if (majorCompare != 0) return majorCompare;

        var minorCompare = Minor.CompareTo(other.Minor);
        if (minorCompare != 0) return minorCompare;

        var patchCompare = Patch.CompareTo(other.Patch);
        if (patchCompare != 0) return patchCompare;

        return ComparePrerelease(Prerelease, other.Prerelease);
    }

    private static int ComparePrerelease(string? a, string? b)
    {
        if (a == null && b == null) return 0;
        if (a == null) return 1;
        if (b == null) return -1;

        var aParts = a.Split('.');
        var bParts = b.Split('.');
        var minLength = Math.Min(aParts.Length, bParts.Length);

        for (var i = 0; i < minLength; i++)
        {
            var aPart = aParts[i];
            var bPart = bParts[i];

            if (int.TryParse(aPart, out var aNum) && int.TryParse(bPart, out var bNum))
            {
                var numCompare = aNum.CompareTo(bNum);
                if (numCompare != 0) return numCompare;
            }
            else
            {
                var strCompare = string.CompareOrdinal(aPart, bPart);
                if (strCompare != 0) return strCompare;
            }
        }

        return aParts.Length.CompareTo(bParts.Length);
    }

    public bool Equals(SemVer? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        
        return Major == other.Major &&
               Minor == other.Minor &&
               Patch == other.Patch &&
               string.Equals(Prerelease, other.Prerelease, StringComparison.Ordinal);
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as SemVer);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Major, Minor, Patch, Prerelease);
    }

    public override string ToString()
    {
        var result = $"{Major}.{Minor}.{Patch}";
        if (Prerelease != null) result += $"-{Prerelease}";
        if (BuildMetadata != null) result += $"+{BuildMetadata}";
        return result;
    }

    public bool IsPrerelease => Prerelease != null;

    public bool IsStable => !IsPrerelease;

    public bool IsGreaterThan(SemVer other)
    {
        return CompareTo(other) > 0;
    }

    public bool IsLessThan(SemVer other)
    {
        return CompareTo(other) < 0;
    }

    public bool IsEqualOrGreaterThan(SemVer other)
    {
        return CompareTo(other) >= 0;
    }

    public bool IsEqualOrLessThan(SemVer other)
    {
        return CompareTo(other) <= 0;
    }

    public static bool operator ==(SemVer? left, SemVer? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(SemVer? left, SemVer? right)
    {
        return !Equals(left, right);
    }

    public static bool operator <(SemVer left, SemVer right)
    {
        return left.CompareTo(right) < 0;
    }

    public static bool operator <=(SemVer left, SemVer right)
    {
        return left.CompareTo(right) <= 0;
    }

    public static bool operator >(SemVer left, SemVer right)
    {
        return left.CompareTo(right) > 0;
    }

    public static bool operator >=(SemVer left, SemVer right)
    {
        return left.CompareTo(right) >= 0;
    }
}