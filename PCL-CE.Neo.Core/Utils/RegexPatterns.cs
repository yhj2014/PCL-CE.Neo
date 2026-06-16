using System.Text.RegularExpressions;

namespace PCL_CE.Neo.Core.Utils;

public static class RegexPatterns
{
    public static readonly Regex TerracottaId = new("([0-9A-Z]{5}-){4}[0-9A-Z]{5}", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    public static readonly Regex NewLine = new(@"\r\n|\n|\r", RegexOptions.Compiled);

    private const string PatternSemVer =
        @"^v?(?<major>0|[1-9]\d*)\.(?<minor>0|[1-9]\d*)\.(?<patch>0|[1-9]\d*)" +
        @"(?:-(?<prerelease>(?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*)(?:\.(?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*))*))?" +
        @"(?:\+(?<build>[0-9a-zA-Z-]+(?:\.[0-9a-zA-Z-]+)*))?$";
    public static readonly Regex SemVer = new(PatternSemVer, RegexOptions.ExplicitCapture | RegexOptions.Compiled);

    private const string PatternHttpUri = @"^https?://(?:\[[^\]\s]+\]|[^/\s?#:]+)(?::\d{1,5})?(?:/[^\s?#]*)?(?:\?[^\s#]*)?(?:#\S*)?$";
    public static readonly Regex HttpUri = new(PatternHttpUri, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

    private const string PatternFullHttpUri =
        @"^(?<scheme>https?)://(?<host>localhost|(?:(?:25[0-5]|2[0-4]\d|1?\d?\d)(?:\.(?:25[0-5]|2[0-4]\d|1?\d?\d)){3})" +
        @"|\[(?<ipv6>[0-9A-Fa-f:.]+)\]|(?:(?:[A-Za-z0-9](?:[A-Za-z0-9\-]{0,61}[A-Za-z0-9])?\.)+(?:[A-Za-z]{2,63}|xn--[" +
        @"A-Za-z0-9\-]{2,59})))(?::(?<port>6553[0-5]|655[0-2]\d|65[0-4]\d{2}|6[0-4]\d{3}|[1-5]\d{4}|[1-9]\d{0,3}))?" +
        @"(?<path>/[^\s?#]*)?(?:\?(?<query>[^\s#]*))?(?:#(?<fragment>[^\s]*))?$";
    public static readonly Regex FullHttpUri = new(PatternFullHttpUri, RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

    public static readonly Regex LastPendingLogPath = new(@"\\LastPending[_]?[^\\]*\.log$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    public static readonly Regex IncompatibleModLoaderErrorHint = new(@"(incompatible[\s\S]+'Fabric Loader' \(fabricloader\)|Mod ID: '(?:neo)?forge', Requested by '([^']+)')", RegexOptions.Compiled);
    public static readonly Regex HexColor = new("^#[0-9A-Fa-f]{6}$", RegexOptions.Compiled);
    public static readonly Regex MotdCode = new("(§[0-9a-fk-oAr]|#[0-9A-Fa-f]{6})", RegexOptions.Compiled);
    public static readonly Regex BroadcastMotd = new(@"\[MOTD\](.*?)\[/MOTD\]", RegexOptions.Compiled);
    public static readonly Regex BroadcastAd = new(@"\[AD\](.*?)\[/AD\]", RegexOptions.Compiled);
    public static readonly Regex McNormalVersion = new(@"^\d+\.\d+\.\d+$|^\d+\.\d+$", RegexOptions.Compiled);
    public static readonly Regex McSnapshotVersion = new(@"(\d+)w(\d+)([a-z]?)", RegexOptions.Compiled);
    public static readonly Regex McIndevVersion = new(@"^in-(\d{8})(-(\d+))?$", RegexOptions.Compiled);
    public static readonly Regex McInfdevVersion = new(@"^inf-(\d{8})(-(\d+))?$", RegexOptions.Compiled);
    public static readonly Regex AccessToken = new("(?<=accessToken ([^ ]{5}))[^ ]+(?=[^ ]{5})", RegexOptions.Compiled);
    public static readonly Regex HasChineseChar = new("[\u4e00-\u9fbb]", RegexOptions.Compiled);
    public static readonly Regex EnglishSpacedKeywords = new("([A-Z]+|[a-z]+?)(?=[A-Z]+[a-z]+[a-z ]*)", RegexOptions.Compiled);
    public static readonly Regex Ntfs83FileName = new(".{2,}~\\d", RegexOptions.Compiled);
    public static readonly Regex UncPath = new("""^\\\\[^\\/:*?"<>|]+\\[^\\/:*?"<>|]+(\\[^\\/:*?"<>|]+)*\\?$""", RegexOptions.Compiled);

    public static readonly Regex OptiFineVersion = new(@"(?<=HD_U_)[^"":/]+", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
    public static readonly Regex OptiFineLibVersion = new(@"(?<=HD_U_)[^"":/]+", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
    public static readonly Regex LegacyFabricVersion = new(@"(?<=(net.fabricmc:fabric-loader:))[0-9\.]+(\+build.[0-9]+)?", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
    public static readonly Regex FabricVersion = new(@"(?<=(net.fabricmc:fabric-loader:))[0-9\.]+(\+build.[0-9]+)?", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
    public static readonly Regex QuiltVersion = new(@"(?<=(org.quiltmc:quilt-loader:))[0-9\.]+(\+build.[0-9]+)?((-beta.)[0-9]([0-9]?))", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
    public static readonly Regex CleanroomVersion = new(@"(?<=(com.cleanroommc:cleanroom:))[0-9\.]+(\+build.[0-9]+)?(-alpha)?", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
    public static readonly Regex ForgeMainVersion = new(@"(?<=forge:[0-9\.]+(_pre[0-9]*)?\-)[0-9\.]+", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
    public static readonly Regex ForgeLibVersion = new(@"(?<=net\.minecraftforge:(?:forge|fmlloader):[0-9.]+-)[0-9a-zA-Z._+-]+", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
    public static readonly Regex NeoForgeVersion = new(@"(?<=orgeVersion"",[^""]*?"")[^""]+(?="",)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
    public static readonly Regex FabricLikeLibVersion = new(@"(?<=((fabricmc)|(quiltmc)|(legacyfabric)):intermediary:)[^""]*", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
    public static readonly Regex LabyModVersion = new(@"(?<=-Dnet.labymod.running-version=)1.[0-9+.]+", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
    public static readonly Regex MinecraftJsonVersion = new(@"(([1-9][0-9]w[0-9]{2}[a-g])|((1|[2-9][0-9])\.[0-9]+(\.[0-9]+)?(-(pre|rc|snapshot-?)[1-9]*| Pre-Release( [1-9])?)?))(_unobfuscated)?", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
    public static readonly Regex MinecraftDownloadUrlVersion = new(@"(?<=launcher.mojang.com/mc/game/)[^/]*", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
    public static readonly Regex CatchLwjglInLib = new(@"(?<=org.lwjgl:)lwjgl(-[a-z._.\-.0-9]*)(?=(:[0-9].[0-9].[0-9](-[a-z.0-9._.\-]*)?:([a-z._.\-.0-9]*)?))", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

    public static readonly Regex DlNeoForgeVersion = new(@"(?<="")(1\.20\.1-)?\d+\.[^\.]+\.\d+(\.\d+)?(-(beta|alpha)(\.\d+)?)?(\+snapshot-\d+)?(\+pre-\d+)?(?="")", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

    public static readonly Regex ModIdMatch = new(@"[0-9a-zA-Z_-]+", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
}