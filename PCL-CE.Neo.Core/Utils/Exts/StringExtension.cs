using System;
using System.Linq;

namespace PCL_CE.Neo.Core.Utils.Exts;

public static class StringExtension
{
    public static bool IsNullOrEmpty(this string? str) => string.IsNullOrEmpty(str);

    public static bool IsNullOrWhiteSpace(this string? str) => string.IsNullOrWhiteSpace(str);

    public static string EmptyIfNull(this string? str) => str ?? string.Empty;

    public static string TrimToNull(this string str)
    {
        var trimmed = str.Trim();
        return string.IsNullOrEmpty(trimmed) ? null! : trimmed;
    }

    public static bool EqualsIgnoreCase(this string str, string other) =>
        string.Equals(str, other, StringComparison.OrdinalIgnoreCase);

    public static bool ContainsIgnoreCase(this string str, string value) =>
        str.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0;

    public static string RemoveWhitespace(this string str) =>
        new(str.Where(c => !char.IsWhiteSpace(c)).ToArray());

    public static string Truncate(this string str, int maxLength, string suffix = "...")
    {
        if (str.Length <= maxLength) return str;
        return str.Substring(0, maxLength - suffix.Length) + suffix;
    }

    public static string Left(this string str, int length) =>
        str.Length <= length ? str : str.Substring(0, length);

    public static string Right(this string str, int length) =>
        str.Length <= length ? str : str.Substring(str.Length - length);

    public static string[] SplitByLength(this string str, int length)
    {
        var parts = new List<string>();
        for (int i = 0; i < str.Length; i += length)
        {
            parts.Add(str.Substring(i, Math.Min(length, str.Length - i)));
        }
        return parts.ToArray();
    }

    public static bool IsValidEmail(this string email)
    {
        if (email.IsNullOrEmpty()) return false;
        var parts = email.Split('@');
        return parts.Length == 2 &&
               !parts[0].IsNullOrEmpty() &&
               !parts[1].IsNullOrEmpty() &&
               parts[1].Contains('.');
    }

    public static bool IsValidUrl(this string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var uri) &&
               (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }
}