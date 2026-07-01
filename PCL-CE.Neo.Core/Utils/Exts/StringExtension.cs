namespace PCL_CE.Neo.Core.Utils.Exts;

public static class StringExtension
{
    public static bool IsNullOrWhiteSpace(this string? str)
    {
        return string.IsNullOrWhiteSpace(str);
    }

    public static bool IsNullOrEmpty(this string? str)
    {
        return string.IsNullOrEmpty(str);
    }

    public static bool IsNotNullOrWhiteSpace(this string? str)
    {
        return !string.IsNullOrWhiteSpace(str);
    }

    public static bool IsNotNullOrEmpty(this string? str)
    {
        return !string.IsNullOrEmpty(str);
    }

    public static string? NullIfEmpty(this string? str)
    {
        return string.IsNullOrEmpty(str) ? null : str;
    }

    public static string? NullIfWhiteSpace(this string? str)
    {
        return string.IsNullOrWhiteSpace(str) ? null : str;
    }

    public static string EmptyIfNull(this string? str)
    {
        return str ?? string.Empty;
    }

    public static string Truncate(this string str, int maxLength, string suffix = "...")
    {
        if (str.Length <= maxLength)
            return str;

        return str.Substring(0, maxLength - suffix.Length) + suffix;
    }

    public static bool EqualsIgnoreCase(this string str, string? other)
    {
        return string.Equals(str, other, StringComparison.OrdinalIgnoreCase);
    }

    public static bool ContainsIgnoreCase(this string str, string? substring)
    {
        if (substring == null)
            return false;

        return str.IndexOf(substring, StringComparison.OrdinalIgnoreCase) >= 0;
    }

    public static string ReplaceIgnoreCase(this string str, string oldValue, string newValue)
    {
        var result = new System.Text.StringBuilder();
        var currentIndex = 0;
        var index = str.IndexOf(oldValue, StringComparison.OrdinalIgnoreCase);

        while (index != -1)
        {
            result.Append(str.Substring(currentIndex, index - currentIndex));
            result.Append(newValue);
            currentIndex = index + oldValue.Length;
            index = str.IndexOf(oldValue, currentIndex, StringComparison.OrdinalIgnoreCase);
        }

        result.Append(str.Substring(currentIndex));
        return result.ToString();
    }

    public static string RemoveWhitespace(this string str)
    {
        return new string(str.Where(c => !char.IsWhiteSpace(c)).ToArray());
    }

    public static string[] SplitByLines(this string str)
    {
        return str.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
    }

    public static string[] SplitByLines(this string str, StringSplitOptions options)
    {
        return str.Split(new[] { "\r\n", "\r", "\n" }, options);
    }

    public static string JoinLines(this IEnumerable<string> lines)
    {
        return string.Join(Environment.NewLine, lines);
    }

    public static bool StartsWithAny(this string str, params string[] prefixes)
    {
        return prefixes.Any(str.StartsWith);
    }

    public static bool EndsWithAny(this string str, params string[] suffixes)
    {
        return suffixes.Any(str.EndsWith);
    }

    public static bool ContainsAny(this string str, params string[] substrings)
    {
        return substrings.Any(str.Contains);
    }

    public static bool ContainsAnyIgnoreCase(this string str, params string[] substrings)
    {
        return substrings.Any(s => str.ContainsIgnoreCase(s));
    }

    public static string TrimEnd(this string str, params char[] trimChars)
    {
        return str.TrimEnd(trimChars);
    }

    public static string TrimStart(this string str, params char[] trimChars)
    {
        return str.TrimStart(trimChars);
    }

    public static string TrimToLength(this string str, int maxLength)
    {
        if (str.Length <= maxLength)
            return str;

        return str.Substring(0, maxLength);
    }
}