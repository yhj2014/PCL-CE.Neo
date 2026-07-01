using System.Text.RegularExpressions;

namespace PCL_CE.Neo.Core.Utils;

public class SemVer : IComparable<SemVer>, IEquatable<SemVer>
{
    private static readonly Regex SemVerRegex = new Regex(
        @"^(?<major>0|[1-9]\d*)\.(?<minor>0|[1-9]\d*)\.(?<patch>0|[1-9]\d*)(?:-(?<prerelease>(?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*)(?:\.(?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*))*))?(?:\+(?<buildmetadata>[0-9a-zA-Z-]+(?:\.[0-9a-zA-Z-]+)*))?$",
        RegexOptions.Compiled);

    public int Major { get; }
    public int Minor { get; }
    public int Patch { get; }
    public string? Prerelease { get; }
    public string? BuildMetadata { get; }

    public SemVer(int major, int minor, int patch, string? prerelease = null, string? buildMetadata = null)
    {
        if (major < 0)
            throw new ArgumentOutOfRangeException(nameof(major), "主版本号不能为负数");

        if (minor < 0)
            throw new ArgumentOutOfRangeException(nameof(minor), "次版本号不能为负数");

        if (patch < 0)
            throw new ArgumentOutOfRangeException(nameof(patch), "补丁版本号不能为负数");

        Major = major;
        Minor = minor;
        Patch = patch;
        Prerelease = prerelease;
        BuildMetadata = buildMetadata;
    }

    public static bool TryParse(string? version, out SemVer? semVer)
    {
        semVer = null;

        if (string.IsNullOrWhiteSpace(version))
            return false;

        var match = SemVerRegex.Match(version);
        if (!match.Success)
            return false;

        if (!int.TryParse(match.Groups["major"].Value, out var major))
            return false;

        if (!int.TryParse(match.Groups["minor"].Value, out var minor))
            return false;

        if (!int.TryParse(match.Groups["patch"].Value, out var patch))
            return false;

        var prerelease = match.Groups["prerelease"].Success ? match.Groups["prerelease"].Value : null;
        var buildMetadata = match.Groups["buildmetadata"].Success ? match.Groups["buildmetadata"].Value : null;

        semVer = new SemVer(major, minor, patch, prerelease, buildMetadata);
        return true;
    }

    public static SemVer Parse(string version)
    {
        if (TryParse(version, out var semVer))
            return semVer;

        throw new FormatException($"无效的 SemVer 格式: {version}");
    }

    public override string ToString()
    {
        var result = $"{Major}.{Minor}.{Patch}";

        if (Prerelease != null)
            result += $"-{Prerelease}";

        if (BuildMetadata != null)
            result += $"+{BuildMetadata}";

        return result;
    }

    public int CompareTo(SemVer? other)
    {
        if (other == null)
            return 1;

        if (Major != other.Major)
            return Major.CompareTo(other.Major);

        if (Minor != other.Minor)
            return Minor.CompareTo(other.Minor);

        if (Patch != other.Patch)
            return Patch.CompareTo(other.Patch);

        return ComparePrerelease(Prerelease, other.Prerelease);
    }

    private static int ComparePrerelease(string? a, string? b)
    {
        if (a == null && b == null)
            return 0;

        if (a == null)
            return 1;

        if (b == null)
            return -1;

        var partsA = a.Split('.');
        var partsB = b.Split('.');
        var minLength = Math.Min(partsA.Length, partsB.Length);

        for (var i = 0; i < minLength; i++)
        {
            var partA = partsA[i];
            var partB = partsB[i];

            var isNumberA = int.TryParse(partA, out var numA);
            var isNumberB = int.TryParse(partB, out var numB);

            if (isNumberA && isNumberB)
            {
                if (numA != numB)
                    return numA.CompareTo(numB);
            }
            else if (!isNumberA && !isNumberB)
            {
                var result = string.CompareOrdinal(partA, partB);
                if (result != 0)
                    return result;
            }
            else
            {
                return isNumberA ? -1 : 1;
            }
        }

        return partsA.Length.CompareTo(partsB.Length);
    }

    public bool Equals(SemVer? other)
    {
        if (other == null)
            return false;

        return Major == other.Major &&
               Minor == other.Minor &&
               Patch == other.Patch &&
               Prerelease == other.Prerelease;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as SemVer);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Major, Minor, Patch, Prerelease);
    }

    public static bool operator ==(SemVer? a, SemVer? b)
    {
        if (ReferenceEquals(a, b))
            return true;

        if (a is null || b is null)
            return false;

        return a.Equals(b);
    }

    public static bool operator !=(SemVer? a, SemVer? b)
    {
        return !(a == b);
    }

    public static bool operator <(SemVer? a, SemVer? b)
    {
        if (a is null)
            return b is not null;

        return a.CompareTo(b) < 0;
    }

    public static bool operator <=(SemVer? a, SemVer? b)
    {
        if (a is null)
            return true;

        return a.CompareTo(b) <= 0;
    }

    public static bool operator >(SemVer? a, SemVer? b)
    {
        if (a is null)
            return false;

        return a.CompareTo(b) > 0;
    }

    public static bool operator >=(SemVer? a, SemVer? b)
    {
        if (a is null)
            return b is null;

        return a.CompareTo(b) >= 0;
    }

    public bool IsPrerelease => Prerelease != null;

    public SemVer WithoutBuildMetadata()
    {
        return new SemVer(Major, Minor, Patch, Prerelease);
    }

    public SemVer WithoutPrerelease()
    {
        return new SemVer(Major, Minor, Patch);
    }
}