using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace PCL_CE.Neo.Core.Utils;

public class VersionRange
{
    public SemVer? MinVersion { get; }
    public SemVer? MaxVersion { get; }
    public bool MinInclusive { get; }
    public bool MaxInclusive { get; }

    public VersionRange(SemVer? minVersion, SemVer? maxVersion, bool minInclusive = true, bool maxInclusive = true)
    {
        MinVersion = minVersion;
        MaxVersion = maxVersion;
        MinInclusive = minInclusive;
        MaxInclusive = maxInclusive;
    }

    public bool IsVersionInRange(SemVer version)
    {
        if (MinVersion != null)
        {
            var minComparison = SemVer.Compare(version, MinVersion);
            if (MinInclusive && minComparison < 0)
                return false;
            if (!MinInclusive && minComparison <= 0)
                return false;
        }

        if (MaxVersion != null)
        {
            var maxComparison = SemVer.Compare(version, MaxVersion);
            if (MaxInclusive && maxComparison > 0)
                return false;
            if (!MaxInclusive && maxComparison >= 0)
                return false;
        }

        return true;
    }

    public bool IsVersionInRange(string versionString)
    {
        if (!SemVer.TryParse(versionString, out var version))
            return false;

        return IsVersionInRange(version);
    }

    public static bool TryParse(string rangeString, out VersionRange? range)
    {
        range = null;

        if (string.IsNullOrWhiteSpace(rangeString))
            return false;

        var trimmed = rangeString.Trim();

        if (trimmed == "*" || trimmed == "any")
        {
            range = new VersionRange(null, null);
            return true;
        }

        if (trimmed.StartsWith("["))
        {
            return ParseBracketRange(trimmed, out range);
        }

        if (trimmed.StartsWith("("))
        {
            return ParseBracketRange(trimmed, out range);
        }

        if (trimmed.Contains("-"))
        {
            return ParseHyphenRange(trimmed, out range);
        }

        return ParseSimpleRange(trimmed, out range);
    }

    private static bool ParseBracketRange(string rangeString, out VersionRange? range)
    {
        range = null;

        var match = Regex.Match(rangeString, @"^([\[\(])(.+?)\s*,\s*(.+?)([\]\)])$");
        if (!match.Success)
            return false;

        var leftBracket = match.Groups[1].Value;
        var minStr = match.Groups[2].Value.Trim();
        var maxStr = match.Groups[3].Value.Trim();
        var rightBracket = match.Groups[4].Value;

        SemVer? minVersion = null;
        SemVer? maxVersion = null;

        if (minStr != "*" && minStr != "")
        {
            if (!SemVer.TryParse(minStr, out minVersion))
                return false;
        }

        if (maxStr != "*" && maxStr != "")
        {
            if (!SemVer.TryParse(maxStr, out maxVersion))
                return false;
        }

        var minInclusive = leftBracket == "[";
        var maxInclusive = rightBracket == "]";

        range = new VersionRange(minVersion, maxVersion, minInclusive, maxInclusive);
        return true;
    }

    private static bool ParseHyphenRange(string rangeString, out VersionRange? range)
    {
        range = null;

        var parts = rangeString.Split('-');
        if (parts.Length != 2)
            return false;

        var minStr = parts[0].Trim();
        var maxStr = parts[1].Trim();

        SemVer? minVersion = null;
        SemVer? maxVersion = null;

        if (!string.IsNullOrEmpty(minStr))
        {
            if (!SemVer.TryParse(minStr, out minVersion))
                return false;
        }

        if (!string.IsNullOrEmpty(maxStr))
        {
            if (!SemVer.TryParse(maxStr, out maxVersion))
                return false;
        }

        range = new VersionRange(minVersion, maxVersion);
        return true;
    }

    private static bool ParseSimpleRange(string rangeString, out VersionRange? range)
    {
        range = null;

        if (rangeString.StartsWith(">="))
        {
            var versionStr = rangeString.Substring(2).Trim();
            if (SemVer.TryParse(versionStr, out var version))
            {
                range = new VersionRange(version, null);
                return true;
            }
            return false;
        }

        if (rangeString.StartsWith(">"))
        {
            var versionStr = rangeString.Substring(1).Trim();
            if (SemVer.TryParse(versionStr, out var version))
            {
                range = new VersionRange(version, null, false);
                return true;
            }
            return false;
        }

        if (rangeString.StartsWith("<="))
        {
            var versionStr = rangeString.Substring(2).Trim();
            if (SemVer.TryParse(versionStr, out var version))
            {
                range = new VersionRange(null, version);
                return true;
            }
            return false;
        }

        if (rangeString.StartsWith("<"))
        {
            var versionStr = rangeString.Substring(1).Trim();
            if (SemVer.TryParse(versionStr, out var version))
            {
                range = new VersionRange(null, version, true, false);
                return true;
            }
            return false;
        }

        if (rangeString.StartsWith("="))
        {
            var versionStr = rangeString.Substring(1).Trim();
            if (SemVer.TryParse(versionStr, out var version))
            {
                range = new VersionRange(version, version);
                return true;
            }
            return false;
        }

        if (SemVer.TryParse(rangeString, out var exactVersion))
        {
            range = new VersionRange(exactVersion, exactVersion);
            return true;
        }

        return false;
    }

    public static VersionRange Parse(string rangeString)
    {
        if (!TryParse(rangeString, out var range))
            throw new FormatException($"Invalid version range: {rangeString}");

        return range!;
    }

    public override string ToString()
    {
        if (MinVersion == null && MaxVersion == null)
            return "*";

        if (MinVersion != null && MaxVersion != null && MinVersion.Equals(MaxVersion))
            return MinVersion.ToString();

        var leftBracket = MinInclusive ? "[" : "(";
        var rightBracket = MaxInclusive ? "]" : ")";
        var minStr = MinVersion?.ToString() ?? "*";
        var maxStr = MaxVersion?.ToString() ?? "*";

        return $"{leftBracket}{minStr}, {maxStr}{rightBracket}";
    }
}