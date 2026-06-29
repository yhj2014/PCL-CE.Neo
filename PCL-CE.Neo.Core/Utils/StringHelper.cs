using System;
using System.Linq;

namespace PCL_CE.Neo.Core.Utils;

public static class StringHelper
{
    public static bool IsNullOrEmpty(string? value)
    {
        return string.IsNullOrEmpty(value);
    }

    public static bool IsNullOrWhiteSpace(string? value)
    {
        return string.IsNullOrWhiteSpace(value);
    }

    public static string? TrimOrNull(string? value)
    {
        return value?.Trim() ?? null;
    }

    public static string TrimOrDefault(string? value, string defaultValue = "")
    {
        return value?.Trim() ?? defaultValue;
    }

    public static string RemoveWhitespace(string value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        return new string(value.Where(c => !char.IsWhiteSpace(c)).ToArray());
    }

    public static string RemoveSpecialCharacters(string value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        return new string(value.Where(c => char.IsLetterOrDigit(c) || char.IsWhiteSpace(c)).ToArray());
    }

    public static bool ContainsIgnoreCase(string source, string value)
    {
        if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(value))
            return false;

        return source.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0;
    }

    public static bool EqualsIgnoreCase(string? a, string? b)
    {
        return string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
    }

    public static string FirstCharToUpper(string value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        return char.ToUpper(value[0]) + value.Substring(1);
    }

    public static string FirstCharToLower(string value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        return char.ToLower(value[0]) + value.Substring(1);
    }

    public static string Truncate(string value, int maxLength, string suffix = "...")
    {
        if (string.IsNullOrEmpty(value))
            return value;

        if (value.Length <= maxLength)
            return value;

        return value.Substring(0, maxLength - suffix.Length) + suffix;
    }

    public static string TruncateMiddle(string value, int maxLength, string middle = "...")
    {
        if (string.IsNullOrEmpty(value))
            return value;

        if (value.Length <= maxLength)
            return value;

        var partLength = (maxLength - middle.Length) / 2;
        return value.Substring(0, partLength) + middle + value.Substring(value.Length - partLength);
    }

    public static string Repeat(string value, int times)
    {
        if (times <= 0)
            return string.Empty;

        return string.Concat(Enumerable.Repeat(value, times));
    }

    public static string PadBoth(string value, int totalWidth, char paddingChar = ' ')
    {
        if (string.IsNullOrEmpty(value))
            return new string(paddingChar, totalWidth);

        var padding = totalWidth - value.Length;
        var leftPadding = padding / 2;
        var rightPadding = padding - leftPadding;

        return new string(paddingChar, leftPadding) + value + new string(paddingChar, rightPadding);
    }

    public static bool IsNumeric(string value)
    {
        return double.TryParse(value, out _);
    }

    public static bool IsInteger(string value)
    {
        return int.TryParse(value, out _);
    }

    public static bool IsEmail(string value)
    {
        if (string.IsNullOrEmpty(value))
            return false;

        try
        {
            var addr = new System.Net.Mail.MailAddress(value);
            return addr.Address == value;
        }
        catch
        {
            return false;
        }
    }

    public static bool IsUrl(string value)
    {
        return Uri.TryCreate(value, UriKind.Absolute, out var uri) && 
               (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }

    public static string ReplaceNewlines(string value, string replacement = " ")
    {
        if (string.IsNullOrEmpty(value))
            return value;

        return value.Replace("\r\n", replacement)
                    .Replace("\r", replacement)
                    .Replace("\n", replacement);
    }

    public static string NormalizeLineEndings(string value, string lineEnding = "\r\n")
    {
        if (string.IsNullOrEmpty(value))
            return value;

        return value.Replace("\r\n", "\n")
                    .Replace("\r", "\n")
                    .Replace("\n", lineEnding);
    }
}