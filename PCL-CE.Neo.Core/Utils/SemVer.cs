using System;
using System.Text.RegularExpressions;

namespace PCL.CE.Neo.Core.Utils;

public class SemVer
{
    private static readonly Regex SemVerRegex = new(
        @"^(?<major>\d+)(\.(?<minor>\d+))?(\.(?<patch>\d+))?(?:-(?<prerelease>[0-9A-Za-z-]+(\.[0-9A-Za-z-]+)*))?(?:\+(?<build>[0-9A-Za-z-]+(\.[0-9A-Za-z-]+)*))?$",
        RegexOptions.Compiled);

    public int Major { get; }
    public int Minor { get; }
    public int Patch { get; }
    public string? PreRelease { get; }
    public string? Build { get; }

    public SemVer(int major, int minor = 0, int patch = 0, string? preRelease = null, string? build = null)
    {
        Major = major;
        Minor = minor;
        Patch = patch;
        PreRelease = preRelease;
        Build = build;
    }

    public static bool TryParse(string input, out SemVer? version)
    {
        version = null;
        if (string.IsNullOrWhiteSpace(input)) return false;

        var match = SemVerRegex.Match(input.Trim());
        if (!match.Success) return false;

        if (!int.TryParse(match.Groups["major"].Value, out var major)) return false;
        
        var minor = 0;
        if (match.Groups["minor"].Success && !int.TryParse(match.Groups["minor"].Value, out minor))
            return false;

        var patch = 0;
        if (match.Groups["patch"].Success && !int.TryParse(match.Groups["patch"].Value, out patch))
            return false;

        var preRelease = match.Groups["prerelease"].Success ? match.Groups["prerelease"].Value : null;
        var build = match.Groups["build"].Success ? match.Groups["build"].Value : null;

        version = new SemVer(major, minor, patch, preRelease, build);
        return true;
    }

    public static SemVer Parse(string input)
    {
        if (!TryParse(input, out var version))
            throw new FormatException($"Invalid semantic version: {input}");
        return version!;
    }

    public int CompareTo(SemVer other)
    {
        if (Major != other.Major) return Major.CompareTo(other.Major);
        if (Minor != other.Minor) return Minor.CompareTo(other.Minor);
        if (Patch != other.Patch) return Patch.CompareTo(other.Patch);
        
        return ComparePreRelease(PreRelease, other.PreRelease);
    }

    private static int ComparePreRelease(string? a, string? b)
    {
        if (a == null && b == null) return 0;
        if (a == null) return 1;
        if (b == null) return -1;

        var partsA = a.Split('.');
        var partsB = b.Split('.');

        for (int i = 0; i < Math.Max(partsA.Length, partsB.Length); i++)
        {
            var partA = i < partsA.Length ? partsA[i] : string.Empty;
            var partB = i < partsB.Length ? partsB[i] : string.Empty;

            if (int.TryParse(partA, out var numA) && int.TryParse(partB, out var numB))
            {
                if (numA != numB) return numA.CompareTo(numB);
            }
            else
            {
                var result = string.CompareOrdinal(partA, partB);
                if (result != 0) return result;
            }
        }

        return 0;
    }

    public override string ToString()
    {
        var result = $"{Major}.{Minor}.{Patch}";
        if (!string.IsNullOrEmpty(PreRelease)) result += $"-{PreRelease}";
        if (!string.IsNullOrEmpty(Build)) result += $"+{Build}";
        return result;
    }

    public override bool Equals(object? obj)
    {
        return obj is SemVer other && CompareTo(other) == 0;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Major, Minor, Patch, PreRelease, Build);
    }

    public static bool operator ==(SemVer? a, SemVer? b)
    {
        if (a is null && b is null) return true;
        if (a is null || b is null) return false;
        return a.CompareTo(b) == 0;
    }

    public static bool operator !=(SemVer? a, SemVer? b)
    {
        return !(a == b);
    }

    public static bool operator <(SemVer a, SemVer b)
    {
        return a.CompareTo(b) < 0;
    }

    public static bool operator <=(SemVer a, SemVer b)
    {
        return a.CompareTo(b) <= 0;
    }

    public static bool operator >(SemVer a, SemVer b)
    {
        return a.CompareTo(b) > 0;
    }

    public static bool operator >=(SemVer a, SemVer b)
    {
        return a.CompareTo(b) >= 0;
    }
}