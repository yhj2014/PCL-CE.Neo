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

    public static readonly Regex LastPendingLogPath = _LastPendingLogPath();
    [GeneratedRegex(@"\\LastPending[_]?[^\\]*\.log$", RegexOptions.IgnoreCase)]
    private static partial Regex _LastPendingLogPath();

    public static readonly Regex IncompatibleModLoaderErrorHint = _IncompatibleModLoaderErrorHint();
    [GeneratedRegex(@"(incompatible[\s\S]+'Fabric Loader' \(fabricloader\)|Mod ID: '(?:neo)?forge', Requested by '([^']+)')")]
    private static partial Regex _IncompatibleModLoaderErrorHint();

    public static readonly Regex HexColor = _HexColor();
    [GeneratedRegex("^#[0-9A-Fa-f]{6}$")]
    private static partial Regex _HexColor();

    public static readonly Regex MotdCode = _MotdCode();
    [GeneratedRegex("(§[0-9a-fk-oAr]|#[0-9A-Fa-f]{6})")]
    private static partial Regex _MotdCode();

    public static readonly Regex BroadcastMotd = _BroadcastMotd();
    [GeneratedRegex(@"\[MOTD\](.*?)\[/MOTD\]", RegexOptions.Compiled)]
    private static partial Regex _BroadcastMotd();

    public static readonly Regex BroadcastAd = _BroadcastAd();
    [GeneratedRegex(@"\[AD\](.*?)\[/AD\]", RegexOptions.Compiled)]
    private static partial Regex _BroadcastAd();

    public static readonly Regex McNormalVersion = _McNormalVersion();
    [GeneratedRegex(@"^\d+\.\d+\.\d+$|^\d+\.\d+$")]
    private static partial Regex _McNormalVersion();

    public static readonly Regex McSnapshotVersion = _McSnapshotVersion();
    [GeneratedRegex(@"(\d+)w(\d+)([a-z]?)")]
    private static partial Regex _McSnapshotVersion();

    public static readonly Regex McIndevVersion = _McIndevVersion();
    [GeneratedRegex(@"^in-(\d{8})(-(\d+))?$")]
    private static partial Regex _McIndevVersion();

    public static readonly Regex McInfdevVersion = _McInfdevVersion();
    [GeneratedRegex(@"^inf-(\d{8})(-(\d+))?$")]
    private static partial Regex _McInfdevVersion();

    public static readonly Regex AccessToken = _AccessToken();
    [GeneratedRegex("(?<=accessToken ([^ ]{5}))[^ ]+(?=[^ ]{5})")]
    private static partial Regex _AccessToken();

    public static readonly Regex HasChineseChar = _HasChineseChar();
    [GeneratedRegex("[\u4e00-\u9fbb]")]
    private static partial Regex _HasChineseChar();

    public static readonly Regex EnglishSpacedKeywords = _EnglishSpacedKeywords();
    [GeneratedRegex("([A-Z]+|[a-z]+?)(?=[A-Z]+[a-z]+[a-z ]*)")]
    private static partial Regex _EnglishSpacedKeywords();

    public static readonly Regex Ntfs83FileName = _Ntfs83FileName();
    [GeneratedRegex(@".{2,}~\d")]
    private static partial Regex _Ntfs83FileName();

    public static readonly Regex UncPath = _UncPath();
    [GeneratedRegex("""^\\[^\\/:*?"<>|]+\\[^\\/:*?"<>|]+(\\[^\\/:*?"<>|]+)*\\?$""")]
    private static partial Regex _UncPath();

    public static readonly Regex OptiFineVersion = _OptiFineVersion();
    [GeneratedRegex(@"(?<=HD_U_)[^"":/]+", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex _OptiFineVersion();

    public static readonly Regex OptiFineLibVersion = _OptiFineLibVersion();
    [GeneratedRegex(@"(?<=HD_U_)[^"":/]+", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex _OptiFineLibVersion();

    public static readonly Regex LegacyFabricVersion = _LegacyFabricVersion();
    [GeneratedRegex(@"(?<=(net.fabricmc:fabric-loader:))[0-9\.]+(\+build.[0-9]+)?", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex _LegacyFabricVersion();

    public static readonly Regex FabricVersion = _FabricVersion();
    [GeneratedRegex(@"(?<=(net.fabricmc:fabric-loader:))[0-9\.]+(\+build.[0-9]+)?", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex _FabricVersion();

    public static readonly Regex QuiltVersion = _QuiltVersion();
    [GeneratedRegex(@"(?<=(org.quiltmc:quilt-loader:))[0-9\.]+(\+build.[0-9]+)?((-beta.)[0-9]([0-9]?))", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex _QuiltVersion();

    public static readonly Regex CleanroomVersion = _CleanroomVersion();
    [GeneratedRegex(@"(?<=(com.cleanroommc:cleanroom:))[0-9\.]+(\+build.[0-9]+)?(-alpha)?", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex _CleanroomVersion();

    public static readonly Regex ForgeMainVersion = _ForgeMainVersion();
    [GeneratedRegex(@"(?<=forge:[0-9\.]+(_pre[0-9]*)?\-)[0-9\.]+", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex _ForgeMainVersion();

    public static readonly Regex ForgeLibVersion = _ForgeLibVersion();
    [GeneratedRegex(@"(?<=net\.minecraftforge:(?:forge|fmlloader):[0-9.]+-)[0-9a-zA-Z._+-]+", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex _ForgeLibVersion();

    public static readonly Regex NeoForgeVersion = _NeoForgeVersion();
    [GeneratedRegex(@"(?<=orgeVersion"",[^""]*?"")[^""]+(?="",)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex _NeoForgeVersion();

    public static readonly Regex FabricLikeLibVersion = _FabricLikeLibVersion();
    [GeneratedRegex(@"(?<=((fabricmc)|(quiltmc)|(legacyfabric)):intermediary:)[^""]*", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex _FabricLikeLibVersion();

    public static readonly Regex LabyModVersion = _LabyModVersion();
    [GeneratedRegex(@"(?<=-Dnet.labymod.running-version=)1.[0-9+.]+", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex _LabyModVersion();

    public static readonly Regex MinecraftJsonVersion = _MinecraftJsonVersion();
    [GeneratedRegex(@"(([1-9][0-9]w[0-9]{2}[a-g])|((1|[2-9][0-9])\.[0-9]+(\.[0-9]+)?(-(pre|rc|snapshot-?)[1-9]*| Pre-Release( [1-9])?)?))(_unobfuscated)?", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex _MinecraftJsonVersion();

    public static readonly Regex MinecraftDownloadUrlVersion = _MinecraftDownloadUrlVersion();
    [GeneratedRegex(@"(?<=launcher.mojang.com/mc/game/)[^/]*", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex _MinecraftDownloadUrlVersion();

    public static readonly Regex CatchLwjglInLib = _CatchLwjglInLib();
    [GeneratedRegex(@"(?<=org.lwjgl:)lwjgl(-[a-z._.\-.0-9]*)(?=(:[0-9].[0-9].[0-9](-[a-z.0-9._.\-]*)?:([a-z._.\-.0-9]*)?))", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex _CatchLwjglInLib();

    public static readonly Regex DlNeoForgeVersion = _DlNeoForgeVersion();
    [GeneratedRegex(@"(?<="")(1\.20\.1-)?\d+\.[^\.]+\.\d+(\.\d+)?(-(beta|alpha)(\.\d+)?)?(\+snapshot-\d+)?(\+pre-\d+)?(?="")", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex _DlNeoForgeVersion();

    public static readonly Regex ModIdMatch = _ModIdMatch();
    [GeneratedRegex(@"[0-9a-zA-Z_-]+", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex _ModIdMatch();
}