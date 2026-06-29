using System.Text.RegularExpressions;

namespace PCL.CE.Neo.Core.Utils.Validate;

public static class RegexPatterns
{
    public static readonly Regex HttpUri = new(
        @"^https?:\/\/(?:www\.)?[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b(?:[-a-zA-Z0-9()@:%_\+.~#?&\/=]*)$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public static readonly Regex HttpAndUncUri = new(
        @"^(?:https?:\/\/(?:www\.)?[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b(?:[-a-zA-Z0-9()@:%_\+.~#?&\/=]*)|\\\\[^\\]+\\[^\\]+)$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public static readonly Regex Ntfs83FileName = new(
        @"^[^.]{1,8}(\.[^.]{1,3})?$",
        RegexOptions.Compiled);

    public static readonly Regex IpAddress = new(
        @"^(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$",
        RegexOptions.Compiled);

    public static readonly Regex PortNumber = new(
        @"^[1-9][0-9]{0,4}$",
        RegexOptions.Compiled);

    public static readonly Regex Email = new(
        @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
        RegexOptions.Compiled);

    public static readonly Regex MinecraftChar = new(
        @"^[^<>:""/\\|?*]*$",
        RegexOptions.Compiled);

    public static bool IsMatch(this string input, Regex regex)
    {
        return !string.IsNullOrEmpty(input) && regex.IsMatch(input);
    }
}