using System;
using System.Text.RegularExpressions;

namespace PCL_CE.Neo.Core.Utils;

public class VersionRange
{
    private static readonly Regex _rangeRegex = new(@"^(\[|\()\s*([^\s,\]]+)\s*,\s*([^\s,\]]+)\s*(\]|\))$", RegexOptions.Compiled);

    public SemVer? MinVersion { get; }
    public SemVer? MaxVersion { get; }
    public bool MinInclusive { get; }
    public bool MaxInclusive { get; }

    public VersionRange(string range)
    {
        if (string.IsNullOrWhiteSpace(range))
            throw new ArgumentNullException(nameof(range));

        var match = _rangeRegex.Match(range.Trim());
        if (!match.Success)
            throw new ArgumentException($"Invalid version range format: {range}", nameof(range));

        var minStr = match.Groups[2].Value;
        var maxStr = match.Groups[3].Value;

        MinInclusive = match.Groups[1].Value == "[";
        MaxInclusive = match.Groups[4].Value == "]";

        MinVersion = minStr == "*" ? null : SemVer.Parse(minStr);
        MaxVersion = maxStr == "*" ? null : SemVer.Parse(maxStr);
    }

    public VersionRange(SemVer? minVersion, SemVer? maxVersion, bool minInclusive = true, bool maxInclusive = false)
    {
        MinVersion = minVersion;
        MaxVersion = maxVersion;
        MinInclusive = minInclusive;
        MaxInclusive = maxInclusive;
    }

    public bool Contains(SemVer version)
    {
        if (version == null)
            throw new ArgumentNullException(nameof(version));

        if (MinVersion != null)
        {
            var compare = version.CompareTo(MinVersion);
            if (MinInclusive && compare < 0)
                return false;
            if (!MinInclusive && compare <= 0)
                return false;
        }

        if (MaxVersion != null)
        {
            var compare = version.CompareTo(MaxVersion);
            if (MaxInclusive && compare > 0)
                return false;
            if (!MaxInclusive && compare >= 0)
                return false;
        }

        return true;
    }

    public bool Contains(string version)
    {
        return Contains(SemVer.Parse(version));
    }

    public bool Overlaps(VersionRange other)
    {
        if (other == null)
            throw new ArgumentNullException(nameof(other));

        if (MinVersion != null && other.MaxVersion != null)
        {
            var compare = MinVersion.CompareTo(other.MaxVersion);
            if (MinInclusive && other.MaxInclusive && compare > 0)
                return false;
            if (!MinInclusive && !other.MaxInclusive && compare >= 0)
                return false;
            if (!MinInclusive && other.MaxInclusive && compare > 0)
                return false;
            if (MinInclusive && !other.MaxInclusive && compare >= 0)
                return false;
        }

        if (MaxVersion != null && other.MinVersion != null)
        {
            var compare = MaxVersion.CompareTo(other.MinVersion);
            if (MaxInclusive && other.MinInclusive && compare < 0)
                return false;
            if (!MaxInclusive && !other.MinInclusive && compare <= 0)
                return false;
            if (!MaxInclusive && other.MinInclusive && compare < 0)
                return false;
            if (MaxInclusive && !other.MinInclusive && compare <= 0)
                return false;
        }

        return true;
    }

    public VersionRange? Intersect(VersionRange other)
    {
        if (other == null)
            throw new ArgumentNullException(nameof(other));

        if (!Overlaps(other))
            return null;

        SemVer? newMin = null;
        bool newMinInclusive = true;

        if (MinVersion == null)
        {
            newMin = other.MinVersion;
            newMinInclusive = other.MinInclusive;
        }
        else if (other.MinVersion == null)
        {
            newMin = MinVersion;
            newMinInclusive = MinInclusive;
        }
        else
        {
            var compare = MinVersion.CompareTo(other.MinVersion);
            if (compare > 0)
            {
                newMin = MinVersion;
                newMinInclusive = MinInclusive;
            }
            else if (compare < 0)
            {
                newMin = other.MinVersion;
                newMinInclusive = other.MinInclusive;
            }
            else
            {
                newMin = MinVersion;
                newMinInclusive = MinInclusive && other.MinInclusive;
            }
        }

        SemVer? newMax = null;
        bool newMaxInclusive = false;

        if (MaxVersion == null)
        {
            newMax = other.MaxVersion;
            newMaxInclusive = other.MaxInclusive;
        }
        else if (other.MaxVersion == null)
        {
            newMax = MaxVersion;
            newMaxInclusive = MaxInclusive;
        }
        else
        {
            var compare = MaxVersion.CompareTo(other.MaxVersion);
            if (compare < 0)
            {
                newMax = MaxVersion;
                newMaxInclusive = MaxInclusive;
            }
            else if (compare > 0)
            {
                newMax = other.MaxVersion;
                newMaxInclusive = other.MaxInclusive;
            }
            else
            {
                newMax = MaxVersion;
                newMaxInclusive = MaxInclusive && other.MaxInclusive;
            }
        }

        return new VersionRange(newMin, newMax, newMinInclusive, newMaxInclusive);
    }

    public override string ToString()
    {
        var minBracket = MinInclusive ? "[" : "(";
        var maxBracket = MaxInclusive ? "]" : ")";
        var minStr = MinVersion?.ToString() ?? "*";
        var maxStr = MaxVersion?.ToString() ?? "*";
        return $"{minBracket}{minStr}, {maxStr}{maxBracket}";
    }

    public override bool Equals(object? obj)
    {
        if (obj is not VersionRange other)
            return false;

        return Equals(MinVersion, other.MinVersion)
            && Equals(MaxVersion, other.MaxVersion)
            && MinInclusive == other.MinInclusive
            && MaxInclusive == other.MaxInclusive;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(MinVersion, MaxVersion, MinInclusive, MaxInclusive);
    }

    public static bool operator ==(VersionRange? left, VersionRange? right)
    {
        if (left is null && right is null)
            return true;
        if (left is null || right is null)
            return false;
        return left.Equals(right);
    }

    public static bool operator !=(VersionRange? left, VersionRange? right)
    {
        return !(left == right);
    }

    public static VersionRange Parse(string range)
    {
        return new VersionRange(range);
    }

    public static bool TryParse(string range, out VersionRange? result)
    {
        try
        {
            result = new VersionRange(range);
            return true;
        }
        catch
        {
            result = null;
            return false;
        }
    }
}