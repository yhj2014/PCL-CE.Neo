using System.Text.RegularExpressions;

namespace PCL_CE.Neo.Core.Utils;

public class VersionRange
{
    public SemVer? MinVersion { get; }
    public SemVer? MaxVersion { get; }
    public bool MinInclusive { get; }
    public bool MaxInclusive { get; }

    private static readonly Regex RangeRegex = new(
        @"^\s*(?<minOp>[><=]?)\s*(?<minVersion>[^,]+)?\s*(?:,\s*(?<maxOp>[><=]?)\s*(?<maxVersion>.+))?\s*$",
        RegexOptions.Compiled);

    public VersionRange(SemVer? minVersion, SemVer? maxVersion, bool minInclusive = true, bool maxInclusive = true)
    {
        MinVersion = minVersion;
        MaxVersion = maxVersion;
        MinInclusive = minInclusive;
        MaxInclusive = maxInclusive;
    }

    public static bool TryParse(string range, out VersionRange? result)
    {
        result = null;
        if (string.IsNullOrEmpty(range))
            return false;

        var match = RangeRegex.Match(range.Trim());
        if (!match.Success)
            return false;

        SemVer? minVersion = null;
        bool minInclusive = true;
        SemVer? maxVersion = null;
        bool maxInclusive = true;

        var minVersionStr = match.Groups["minVersion"].Value.Trim();
        var minOp = match.Groups["minOp"].Value.Trim();

        if (!string.IsNullOrEmpty(minVersionStr))
        {
            if (!SemVer.TryParse(minVersionStr, out minVersion))
                return false;

            if (minOp == ">")
                minInclusive = false;
            else if (minOp == ">=")
                minInclusive = true;
        }

        var maxVersionStr = match.Groups["maxVersion"].Value.Trim();
        var maxOp = match.Groups["maxOp"].Value.Trim();

        if (!string.IsNullOrEmpty(maxVersionStr))
        {
            if (!SemVer.TryParse(maxVersionStr, out maxVersion))
                return false;

            if (maxOp == "<")
                maxInclusive = false;
            else if (maxOp == "<=")
                maxInclusive = true;
        }

        if (minVersion == null && maxVersion == null)
            return false;

        result = new VersionRange(minVersion, maxVersion, minInclusive, maxInclusive);
        return true;
    }

    public static VersionRange Parse(string range)
    {
        if (!TryParse(range, out var result))
            throw new FormatException($"Invalid version range: {range}");
        return result!;
    }

    public bool IsInRange(SemVer version)
    {
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

    public override string ToString()
    {
        var parts = new List<string>();

        if (MinVersion != null)
        {
            parts.Add(MinInclusive ? $">= {MinVersion}" : $"> {MinVersion}");
        }

        if (MaxVersion != null)
        {
            parts.Add(MaxInclusive ? $"<={MaxVersion}" : $"< {MaxVersion}");
        }

        return string.Join(", ", parts);
    }
}