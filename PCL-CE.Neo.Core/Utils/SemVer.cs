using System.Text.RegularExpressions;

namespace PCL_CE.Neo.Core.Utils;

public class SemVer : IComparable<SemVer>, IEquatable<SemVer>
{
    public int Major { get; }
    public int Minor { get; }
    public int Patch { get; }
    public string? Prerelease { get; }
    public string? Build { get; }

    private static readonly Regex SemVerRegex = new(
        @"^(?<major>0|[1-9]\d*)\.(?<minor>0|[1-9]\d*)\.(?<patch>0|[1-9]\d*)(?:-(?<prerelease>(?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*)(?:\.(?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*))*))?(?:\+(?<build>[0-9a-zA-Z-]+(?:\.[0-9a-zA-Z-]+)*))?$",
        RegexOptions.Compiled);

    public SemVer(int major, int minor, int patch, string? prerelease = null, string? build = null)
    {
        Major = major;
        Minor = minor;
        Patch = patch;
        Prerelease = prerelease;
        Build = build;
    }

    public static bool TryParse(string version, out SemVer? result)
    {
        result = null;
        if (string.IsNullOrEmpty(version))
            return false;

        var match = SemVerRegex.Match(version.Trim());
        if (!match.Success)
            return false;

        if (!int.TryParse(match.Groups["major"].Value, out var major))
            return false;
        if (!int.TryParse(match.Groups["minor"].Value, out var minor))
            return false;
        if (!int.TryParse(match.Groups["patch"].Value, out var patch))
            return false;

        result = new SemVer(major, minor, patch,
            match.Groups["prerelease"].Success ? match.Groups["prerelease"].Value : null,
            match.Groups["build"].Success ? match.Groups["build"].Value : null);

        return true;
    }

    public static SemVer Parse(string version)
    {
        if (!TryParse(version, out var result))
            throw new FormatException($"Invalid semantic version: {version}");
        return result!;
    }

    public override string ToString()
    {
        var version = $"{Major}.{Minor}.{Patch}";
        if (!string.IsNullOrEmpty(Prerelease))
            version += $"-{Prerelease}";
        if (!string.IsNullOrEmpty(Build))
            version += $"+{Build}";
        return version;
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

        var aParts = a.Split('.');
        var bParts = b.Split('.');

        for (int i = 0; i < Math.Max(aParts.Length, bParts.Length); i++)
        {
            var aPart = i < aParts.Length ? aParts[i] : string.Empty;
            var bPart = i < bParts.Length ? bParts[i] : string.Empty;

            if (string.IsNullOrEmpty(aPart) && string.IsNullOrEmpty(bPart))
                continue;
            if (string.IsNullOrEmpty(aPart))
                return -1;
            if (string.IsNullOrEmpty(bPart))
                return 1;

            var aIsNumeric = int.TryParse(aPart, out var aNum);
            var bIsNumeric = int.TryParse(bPart, out var bNum);

            if (aIsNumeric && bIsNumeric)
            {
                var numCompare = aNum.CompareTo(bNum);
                if (numCompare != 0)
                    return numCompare;
            }
            else if (aIsNumeric)
            {
                return -1;
            }
            else if (bIsNumeric)
            {
                return 1;
            }
            else
            {
                var strCompare = string.Compare(aPart, bPart, StringComparison.OrdinalIgnoreCase);
                if (strCompare != 0)
                    return strCompare;
            }
        }

        return 0;
    }

    public bool Equals(SemVer? other)
    {
        if (other == null)
            return false;
        return Major == other.Major && Minor == other.Minor && Patch == other.Patch &&
               Prerelease == other.Prerelease && Build == other.Build;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as SemVer);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Major, Minor, Patch, Prerelease, Build);
    }

    public static bool operator ==(SemVer? left, SemVer? right)
    {
        if (left is null)
            return right is null;
        return left.Equals(right);
    }

    public static bool operator !=(SemVer? left, SemVer? right)
    {
        return !(left == right);
    }

    public static bool operator <(SemVer? left, SemVer? right)
    {
        if (left is null)
            return right is not null;
        return left.CompareTo(right) < 0;
    }

    public static bool operator <=(SemVer? left, SemVer? right)
    {
        if (left is null)
            return true;
        return left.CompareTo(right) <= 0;
    }

    public static bool operator >(SemVer? left, SemVer? right)
    {
        if (left is null)
            return false;
        return left.CompareTo(right) > 0;
    }

    public static bool operator >=(SemVer? left, SemVer? right)
    {
        if (left is null)
            return right is null;
        return left.CompareTo(right) >= 0;
    }
}