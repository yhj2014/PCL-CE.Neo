using System;
using System.Linq;

namespace PCL_CE.Neo.Core.Utils.Exts;

public static class StringExtension
{
    public static bool IsNullOrEmpty(this string? value)
    {
        return string.IsNullOrEmpty(value);
    }

    public static bool IsNullOrWhiteSpace(this string? value)
    {
        return string.IsNullOrWhiteSpace(value);
    }

    public static string? TrimOrNull(this string? value)
    {
        return value?.Trim() ?? null;
    }

    public static string TrimOrDefault(this string? value, string defaultValue = "")
    {
        return value?.Trim() ?? defaultValue;
    }

    public static string RemoveWhitespace(this string value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        return new string(value.Where(c => !char.IsWhiteSpace(c)).ToArray());
    }

    public static bool ContainsIgnoreCase(this string source, string value)
    {
        if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(value))
            return false;

        return source.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0;
    }

    public static bool EqualsIgnoreCase(this string? a, string? b)
    {
        return string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
    }

    public static string FirstCharToUpper(this string value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        return char.ToUpper(value[0]) + value.Substring(1);
    }

    public static string FirstCharToLower(this string value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        return char.ToLower(value[0]) + value.Substring(1);
    }

    public static string Truncate(this string value, int maxLength, string suffix = "...")
    {
        if (string.IsNullOrEmpty(value))
            return value;

        if (value.Length <= maxLength)
            return value;

        return value.Substring(0, maxLength - suffix.Length) + suffix;
    }

    public static string TruncateMiddle(this string value, int maxLength, string middle = "...")
    {
        if (string.IsNullOrEmpty(value))
            return value;

        if (value.Length <= maxLength)
            return value;

        var partLength = (maxLength - middle.Length) / 2;
        return value.Substring(0, partLength) + middle + value.Substring(value.Length - partLength);
    }

    public static string Repeat(this string value, int times)
    {
        if (times <= 0)
            return string.Empty;

        return string.Concat(Enumerable.Repeat(value, times));
    }

    public static bool IsNumeric(this string value)
    {
        return double.TryParse(value, out _);
    }

    public static bool IsInteger(this string value)
    {
        return int.TryParse(value, out _);
    }

    public static bool IsEmail(this string value)
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

    public static bool IsUrl(this string value)
    {
        return Uri.TryCreate(value, UriKind.Absolute, out var uri) && 
               (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }

    public static string ReplaceNewlines(this string value, string replacement = " ")
    {
        if (string.IsNullOrEmpty(value))
            return value;

        return value.Replace("\r\n", replacement)
                    .Replace("\r", replacement)
                    .Replace("\n", replacement);
    }

    public static string NormalizeLineEndings(this string value, string lineEnding = "\r\n")
    {
        if (string.IsNullOrEmpty(value))
            return value;

        return value.Replace("\r\n", "\n")
                    .Replace("\r", "\n")
                    .Replace("\n", lineEnding);
    }

    public static string EnsureEndsWith(this string value, string suffix)
    {
        if (string.IsNullOrEmpty(value))
            return suffix;

        return value.EndsWith(suffix) ? value : value + suffix;
    }

    public static string EnsureStartsWith(this string value, string prefix)
    {
        if (string.IsNullOrEmpty(value))
            return prefix;

        return value.StartsWith(prefix) ? value : prefix + value;
    }

    public static string RemoveEnding(this string value, string suffix)
    {
        if (string.IsNullOrEmpty(value) || string.IsNullOrEmpty(suffix))
            return value;

        return value.EndsWith(suffix) ? value.Substring(0, value.Length - suffix.Length) : value;
    }

    public static string RemoveStarting(this string value, string prefix)
    {
        if (string.IsNullOrEmpty(value) || string.IsNullOrEmpty(prefix))
            return value;

        return value.StartsWith(prefix) ? value.Substring(prefix.Length) : value;
    }

    public static string[] SplitAndTrim(this string value, params char[] separators)
    {
        if (string.IsNullOrEmpty(value))
            return Array.Empty<string>();

        return value.Split(separators, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim())
                    .ToArray();
    }

    public static string[] SplitAndTrim(this string value, string separator)
    {
        if (string.IsNullOrEmpty(value))
            return Array.Empty<string>();

        return value.Split(new[] { separator }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim())
                    .ToArray();
    }
}