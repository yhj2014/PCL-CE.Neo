using System.Text.RegularExpressions;

namespace PCL_CE.Neo.Core.Utils;

public static partial class RegexPatterns
{
    public static readonly Regex TerracottaId = _TerracottaId();
    [GeneratedRegex("([0-9A-Z]{5}-){4}[0-9A-Z]{5}", RegexOptions.IgnoreCase)]
    private static partial Regex _TerracottaId();

    public static readonly Regex NewLine = _NewLine();
    [GeneratedRegex(@"\r\n|\n|\r")]
    private static partial Regex _NewLine();

    public static readonly Regex SemVer = _SemVer();
    private const string PatternSemVer =
        @"^v?(?<major>0|[1-9]\d*)\.(?<minor>0|[1-9]\d*)\.(?<patch>0|[1-9]\d*)" +
        @"(?:-(?<prerelease>(?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*)(?:\.(?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*))*))?" +
        @"(?:\+(?<build>[0-9a-zA-Z-]+(?:\.[0-9a-zA-Z-]+)*))?$";
    [GeneratedRegex(PatternSemVer, RegexOptions.ExplicitCapture)]
    private static partial Regex _SemVer();

    public static readonly Regex HttpUri = _HttpUri();
    private const string PatternHttpUri = @"^https?://(?:\[[^\]\s]+\]|[^/\s?#:]+)(?::\d{1,5})?(?:/[^\s?#]*)?(?:\?[^\s#]*)?(?:#\S*)?$";
    [GeneratedRegex(PatternHttpUri, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex _HttpUri();

    public static readonly Regex FullHttpUri = _FullHttpUri();
    private const string PatternFullHttpUri =
        @"^(?<scheme>https?)://(?<host>localhost|(?:(?:25[0-5]|2[0-4]\d|1?\d?\d)(?:\.(?:25[0-5]|2[0-4]\d|1?\d?\d)){3})" +
        @"|\[(?<ipv6>[0-9A-Fa-f:.]+)\]|(?:(?:[A-Za-z0-9](?:[A-Za-z0-9\-]{0,61}[A-Za-z0-9])?\.)+(?:[A-Za-z]{2,63}|xn--[" +
        @"A-Za-z0-9\-]{2,59})))(?::(?<port>6553[0-5]|655[0-2]\d|65[0-4]\d{2}|6[0-4]\d{3}|[1-5]\d{4}|[1-9]\d{0,3}))?" +
        @"(?<path>/[^\s?#]*)?(?:\?(?<query>[^\s#]*))?(?:#(?<fragment>[^\s]*))?$";
    [GeneratedRegex(PatternFullHttpUri, RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex _FullHttpUri();

    public static readonly Regex HexColor = _HexColor();
    [GeneratedRegex("^#[0-9A-Fa-f]{6}$")]
    private static partial Regex _HexColor();

    public static readonly Regex MotdCode = _MotdCode();
    [GeneratedRegex("(§[0-9a-fk-oAr]|#[0-9A-Fa-f]{6})")]
    private static partial Regex _MotdCode();

    public static readonly Regex McNormalVersion = _McNormalVersion();
    [GeneratedRegex(@"^\d+\.\d+\.\d+$|^\d+\.\d+$")]
    private static partial Regex _McNormalVersion();

    public static readonly Regex McSnapshotVersion = _McSnapshotVersion();
    [GeneratedRegex(@"(\d+)w(\d+)([a-z]?)")]
    private static partial Regex _McSnapshotVersion();

    public static readonly Regex HasChineseChar = _HasChineseChar();
    [GeneratedRegex("[\u4e00-\u9fbb]")]
    private static partial Regex _HasChineseChar();

    public static readonly Regex UncPath = _UncPath();
    [GeneratedRegex("""^\\\\[^\\/:*?"<>|]+\\[^\\/:*?"<>|]+(\\[^\\/:*?"<>|]+)*\\?$""")]
    private static partial Regex _UncPath();

    public static readonly Regex OptiFineVersion = _OptiFineVersion();
    [GeneratedRegex(@"(?<=HD_U_)[^"":/]+", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex _OptiFineVersion();

    public static readonly Regex FabricVersion = _FabricVersion();
    [GeneratedRegex(@"(?<=(net.fabricmc:fabric-loader:))[0-9\.]+(\+build.[0-9]+)?", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex _FabricVersion();

    public static readonly Regex QuiltVersion = _QuiltVersion();
    [GeneratedRegex(@"(?<=(org.quiltmc:quilt-loader:))[0-9\.]+(\+build.[0-9]+)?((-beta.)[0-9]([0-9]?))", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex _QuiltVersion();

    public static readonly Regex ForgeMainVersion = _ForgeMainVersion();
    [GeneratedRegex(@"(?<=forge:[0-9\.]+(_pre[0-9]*)?\-)[0-9\.]+", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex _ForgeMainVersion();

    public static readonly Regex NeoForgeVersion = _NeoForgeVersion();
    [GeneratedRegex(@"(?<=orgeVersion"",[^""]*?"")[^""]+(?="",)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex _NeoForgeVersion();

    public static readonly Regex MinecraftJsonVersion = _MinecraftJsonVersion();
    [GeneratedRegex(@"(([1-9][0-9]w[0-9]{2}[a-g])|((1|[2-9][0-9])\.[0-9]+(\.[0-9]+)?(-(pre|rc|snapshot-?)[1-9]*| Pre-Release( [1-9])?)?))(_unobfuscated)?", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex _MinecraftJsonVersion();

    public static readonly Regex ModIdMatch = _ModIdMatch();
    [GeneratedRegex(@"[0-9a-zA-Z_-]+", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex _ModIdMatch();
}