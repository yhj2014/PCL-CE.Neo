using System.Text.RegularExpressions;

namespace PCL_CE.Neo.Core.Utils;

public static partial class RegexPatterns
{
    public static readonly Regex BroadcastMotd = BroadcastMotdRegex();
    public static readonly Regex BroadcastAd = BroadcastAdRegex();

    [GeneratedRegex(@"\[MOTD\](.*?)\[/MOTD\]", RegexOptions.Compiled)]
    private static partial Regex BroadcastMotdRegex();

    [GeneratedRegex(@"\[AD\](.*?)\[/AD\]", RegexOptions.Compiled)]
    private static partial Regex BroadcastAdRegex();
}
