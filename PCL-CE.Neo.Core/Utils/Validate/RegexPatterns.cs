using System.Text.RegularExpressions;

namespace PCL_CE.Neo.Core.Utils;

public static partial class RegexPatterns
{
    public static readonly Regex HttpUri = _HttpUri();
    private const string PatternHttpUri = @"^https?://(?:\[[^\]\s]+\]|[^/\s?#:]+)(?::\d{1,5})?(?:/[^\s?#]*)?(?:\?[^\s#]*)?(?:#\S*)?$";
    [GeneratedRegex(PatternHttpUri, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex _HttpUri();

    public static readonly Regex Ntfs83FileName = _Ntfs83FileName();
    [GeneratedRegex(@".{2,}~\d")]
    private static partial Regex _Ntfs83FileName();

    public static readonly Regex UncPath = _UncPath();
    [GeneratedRegex("""^\\\\[^\\/:*?"<>|]+\\[^\\/:*?"<>|]+(\\[^\\/:*?"<>|]+)*\\?$""")]
    private static partial Regex _UncPath();

    public static readonly Regex SemVer = _SemVer();
    private const string PatternSemVer =
        @"^v?(?<major>0|[1-9]\d*)\.(?<minor>0|[1-9]\d*)\.(?<patch>0|[1-9]\d*)" +
        @"(?:-(?<prerelease>(?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*)(?:\.(?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*))*))?" +
        @"(?:\+(?<build>[0-9a-zA-Z-]+(?:\.[0-9a-zA-Z-]+)*))?$";
    [GeneratedRegex(PatternSemVer, RegexOptions.ExplicitCapture)]
    private static partial Regex _SemVer();

    public static readonly Regex HexColor = _HexColor();
    [GeneratedRegex("^#[0-9A-Fa-f]{6}$")]
    private static partial Regex _HexColor();
}
