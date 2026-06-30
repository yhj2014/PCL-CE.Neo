using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace PCL_CE.Neo.Core.Utils;

public static class StringUtils
{
    public static bool IsNullOrEmpty(string? value)
    {
        return string.IsNullOrEmpty(value);
    }

    public static bool IsNullOrWhiteSpace(string? value)
    {
        return string.IsNullOrWhiteSpace(value);
    }

    public static string EmptyIfNull(string? value)
    {
        return value ?? string.Empty;
    }

    public static string DefaultIfEmpty(string? value, string defaultValue)
    {
        return string.IsNullOrEmpty(value) ? defaultValue : value;
    }

    public static string DefaultIfNullOrWhiteSpace(string? value, string defaultValue)
    {
        return string.IsNullOrWhiteSpace(value) ? defaultValue : value;
    }

    public static string TrimToNull(string? value)
    {
        var trimmed = value?.Trim();
        return string.IsNullOrEmpty(trimmed) ? null : trimmed;
    }

    public static string TrimToEmpty(string? value)
    {
        return value?.Trim() ?? string.Empty;
    }

    public static string Left(string value, int length)
    {
        if (string.IsNullOrEmpty(value))
            return value ?? string.Empty;

        return length >= value.Length ? value : value.Substring(0, length);
    }

    public static string Right(string value, int length)
    {
        if (string.IsNullOrEmpty(value))
            return value ?? string.Empty;

        return length >= value.Length ? value : value.Substring(value.Length - length);
    }

    public static string Mid(string value, int startIndex)
    {
        if (string.IsNullOrEmpty(value))
            return value ?? string.Empty;

        return startIndex >= value.Length ? string.Empty : value.Substring(startIndex);
    }

    public static string Mid(string value, int startIndex, int length)
    {
        if (string.IsNullOrEmpty(value))
            return value ?? string.Empty;

        if (startIndex >= value.Length)
            return string.Empty;

        return startIndex + length >= value.Length 
            ? value.Substring(startIndex) 
            : value.Substring(startIndex, length);
    }

    public static string FirstCharToUpper(string value)
    {
        if (string.IsNullOrEmpty(value))
            return value ?? string.Empty;

        if (value.Length == 1)
            return value.ToUpper();

        return char.ToUpper(value[0]) + value.Substring(1);
    }

    public static string FirstCharToLower(string value)
    {
        if (string.IsNullOrEmpty(value))
            return value ?? string.Empty;

        if (value.Length == 1)
            return value.ToLower();

        return char.ToLower(value[0]) + value.Substring(1);
    }

    public static string Capitalize(string value)
    {
        if (string.IsNullOrEmpty(value))
            return value ?? string.Empty;

        return FirstCharToUpper(value.ToLower());
    }

    public static string CamelCaseToSnakeCase(string value)
    {
        if (string.IsNullOrEmpty(value))
            return value ?? string.Empty;

        var result = new StringBuilder();
        result.Append(char.ToLower(value[0]));

        for (int i = 1; i < value.Length; i++)
        {
            if (char.IsUpper(value[i]))
            {
                result.Append('_');
                result.Append(char.ToLower(value[i]));
            }
            else
            {
                result.Append(value[i]);
            }
        }

        return result.ToString();
    }

    public static string SnakeCaseToCamelCase(string value)
    {
        if (string.IsNullOrEmpty(value))
            return value ?? string.Empty;

        var words = value.Split('_');
        var result = new StringBuilder();

        for (int i = 0; i < words.Length; i++)
        {
            if (i == 0)
            {
                result.Append(words[i].ToLower());
            }
            else
            {
                result.Append(FirstCharToUpper(words[i].ToLower()));
            }
        }

        return result.ToString();
    }

    public static string CamelCaseToPascalCase(string value)
    {
        if (string.IsNullOrEmpty(value))
            return value ?? string.Empty;

        return FirstCharToUpper(value);
    }

    public static string PascalCaseToCamelCase(string value)
    {
        if (string.IsNullOrEmpty(value))
            return value ?? string.Empty;

        return FirstCharToLower(value);
    }

    public static string CamelCaseToKebabCase(string value)
    {
        if (string.IsNullOrEmpty(value))
            return value ?? string.Empty;

        var result = new StringBuilder();
        result.Append(char.ToLower(value[0]));

        for (int i = 1; i < value.Length; i++)
        {
            if (char.IsUpper(value[i]))
            {
                result.Append('-');
                result.Append(char.ToLower(value[i]));
            }
            else
            {
                result.Append(value[i]);
            }
        }

        return result.ToString();
    }

    public static string KebabCaseToCamelCase(string value)
    {
        if (string.IsNullOrEmpty(value))
            return value ?? string.Empty;

        var words = value.Split('-');
        var result = new StringBuilder();

        for (int i = 0; i < words.Length; i++)
        {
            if (i == 0)
            {
                result.Append(words[i].ToLower());
            }
            else
            {
                result.Append(FirstCharToUpper(words[i].ToLower()));
            }
        }

        return result.ToString();
    }

    public static string Repeat(string value, int count)
    {
        if (string.IsNullOrEmpty(value))
            return value ?? string.Empty;

        if (count <= 0)
            return string.Empty;

        return new StringBuilder(value.Length * count).Insert(0, value, count).ToString();
    }

    public static string PadBoth(string value, int length)
    {
        if (string.IsNullOrEmpty(value))
            return value ?? string.Empty;

        if (value.Length >= length)
            return value;

        var padding = length - value.Length;
        var leftPadding = padding / 2;
        var rightPadding = padding - leftPadding;

        return Repeat(" ", leftPadding) + value + Repeat(" ", rightPadding);
    }

    public static string Truncate(string value, int maxLength, string suffix = "...")
    {
        if (string.IsNullOrEmpty(value))
            return value ?? string.Empty;

        if (value.Length <= maxLength)
            return value;

        return value.Substring(0, maxLength - suffix.Length) + suffix;
    }

    public static string TruncateMiddle(string value, int maxLength, string middle = "...")
    {
        if (string.IsNullOrEmpty(value))
            return value ?? string.Empty;

        if (value.Length <= maxLength)
            return value;

        var availableLength = maxLength - middle.Length;
        var leftLength = availableLength / 2;
        var rightLength = availableLength - leftLength;

        return value.Substring(0, leftLength) + middle + value.Substring(value.Length - rightLength);
    }

    public static string RemoveSpecialCharacters(string value)
    {
        if (string.IsNullOrEmpty(value))
            return value ?? string.Empty;

        return Regex.Replace(value, @"[^a-zA-Z0-9\u4e00-\u9fa5]", "");
    }

    public static string RemoveNonAlphanumeric(string value)
    {
        if (string.IsNullOrEmpty(value))
            return value ?? string.Empty;

        return Regex.Replace(value, @"[^a-zA-Z0-9]", "");
    }

    public static string RemoveDiacritics(string value)
    {
        if (string.IsNullOrEmpty(value))
            return value ?? string.Empty;

        var normalized = value.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder();

        foreach (var c in normalized)
        {
            var category = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
            if (category != System.Globalization.UnicodeCategory.NonSpacingMark)
            {
                builder.Append(c);
            }
        }

        return builder.ToString().Normalize(NormalizationForm.FormC);
    }

    public static bool ContainsIgnoreCase(string value, string substring)
    {
        if (string.IsNullOrEmpty(value) || string.IsNullOrEmpty(substring))
            return false;

        return value.IndexOf(substring, StringComparison.OrdinalIgnoreCase) >= 0;
    }

    public static bool StartsWithIgnoreCase(string value, string prefix)
    {
        if (string.IsNullOrEmpty(value) || string.IsNullOrEmpty(prefix))
            return false;

        return value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
    }

    public static bool EndsWithIgnoreCase(string value, string suffix)
    {
        if (string.IsNullOrEmpty(value) || string.IsNullOrEmpty(suffix))
            return false;

        return value.EndsWith(suffix, StringComparison.OrdinalIgnoreCase);
    }

    public static string ReplaceIgnoreCase(string value, string oldValue, string newValue)
    {
        if (string.IsNullOrEmpty(value))
            return value ?? string.Empty;

        if (string.IsNullOrEmpty(oldValue))
            return value;

        return Regex.Replace(value, Regex.Escape(oldValue), newValue, RegexOptions.IgnoreCase);
    }

    public static string Join<T>(string separator, IEnumerable<T> values)
    {
        if (values == null)
            return string.Empty;

        return string.Join(separator, values);
    }

    public static string Join(string separator, params string[] values)
    {
        if (values == null || values.Length == 0)
            return string.Empty;

        return string.Join(separator, values.Where(v => !string.IsNullOrEmpty(v)));
    }

    public static string[] Split(string value, params char[] separators)
    {
        if (string.IsNullOrEmpty(value))
            return Array.Empty<string>();

        return value.Split(separators, StringSplitOptions.RemoveEmptyEntries);
    }

    public static string[] Split(string value, string separator)
    {
        if (string.IsNullOrEmpty(value))
            return Array.Empty<string>();

        return value.Split(new[] { separator }, StringSplitOptions.RemoveEmptyEntries);
    }

    public static string[] SplitLines(string value)
    {
        if (string.IsNullOrEmpty(value))
            return Array.Empty<string>();

        return value.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
    }

    public static string[] SplitWords(string value)
    {
        if (string.IsNullOrEmpty(value))
            return Array.Empty<string>();

        return Regex.Split(value, @"[\s.,;!?(){}[\]<>""'`~!@#$%^&*+=|\\/-]+")
            .Where(w => !string.IsNullOrEmpty(w))
            .ToArray();
    }

    public static string StripHtml(string value)
    {
        if (string.IsNullOrEmpty(value))
            return value ?? string.Empty;

        return Regex.Replace(value, @"<[^>]*>", string.Empty);
    }

    public static string StripXml(string value)
    {
        if (string.IsNullOrEmpty(value))
            return value ?? string.Empty;

        return Regex.Replace(value, @"<[^>]*>", string.Empty);
    }

    public static string RemoveHtmlTags(string value)
    {
        return StripHtml(value);
    }

    public static string RemoveXmlTags(string value)
    {
        return StripXml(value);
    }

    public static bool IsEmail(string value)
    {
        if (string.IsNullOrEmpty(value))
            return false;

        var pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
        return Regex.IsMatch(value, pattern);
    }

    public static bool IsUrl(string value)
    {
        if (string.IsNullOrEmpty(value))
            return false;

        return Uri.TryCreate(value, UriKind.Absolute, out _);
    }

    public static bool IsIpAddress(string value)
    {
        if (string.IsNullOrEmpty(value))
            return false;

        return System.Net.IPAddress.TryParse(value, out _);
    }

    public static bool IsPhoneNumber(string value)
    {
        if (string.IsNullOrEmpty(value))
            return false;

        var pattern = @"^\+?[1-9]\d{1,14}$";
        return Regex.IsMatch(value, pattern);
    }

    public static bool IsGuid(string value)
    {
        if (string.IsNullOrEmpty(value))
            return false;

        return Guid.TryParse(value, out _);
    }

    public static bool IsNumeric(string value)
    {
        if (string.IsNullOrEmpty(value))
            return false;

        return double.TryParse(value, out _);
    }

    public static bool IsInteger(string value)
    {
        if (string.IsNullOrEmpty(value))
            return false;

        return int.TryParse(value, out _);
    }

    public static bool IsHexString(string value)
    {
        if (string.IsNullOrEmpty(value))
            return false;

        var pattern = @"^[0-9a-fA-F]+$";
        return Regex.IsMatch(value, pattern);
    }

    public static bool IsBase64String(string value)
    {
        if (string.IsNullOrEmpty(value))
            return false;

        try
        {
            Convert.FromBase64String(value);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static string Reverse(string value)
    {
        if (string.IsNullOrEmpty(value))
            return value ?? string.Empty;

        var charArray = value.ToCharArray();
        Array.Reverse(charArray);
        return new string(charArray);
    }

    public static string Shuffle(string value)
    {
        if (string.IsNullOrEmpty(value))
            return value ?? string.Empty;

        var charArray = value.ToCharArray();
        var random = new Random();

        for (int i = charArray.Length - 1; i > 0; i--)
        {
            var j = random.Next(i + 1);
            (charArray[i], charArray[j]) = (charArray[j], charArray[i]);
        }

        return new string(charArray);
    }

    public static string GenerateRandomString(int length, bool includeLowercase = true, 
        bool includeUppercase = true, bool includeNumbers = true, bool includeSpecialChars = false)
    {
        if (length <= 0)
            return string.Empty;

        var chars = new StringBuilder();

        if (includeLowercase)
            chars.Append("abcdefghijklmnopqrstuvwxyz");

        if (includeUppercase)
            chars.Append("ABCDEFGHIJKLMNOPQRSTUVWXYZ");

        if (includeNumbers)
            chars.Append("0123456789");

        if (includeSpecialChars)
            chars.Append("!@#$%^&*()_+-=[]{}|;:,.<>?");

        if (chars.Length == 0)
            return string.Empty;

        var random = new Random();
        var result = new StringBuilder(length);

        for (int i = 0; i < length; i++)
        {
            result.Append(chars[random.Next(chars.Length)]);
        }

        return result.ToString();
    }

    public static int CountOccurrences(string value, char character)
    {
        if (string.IsNullOrEmpty(value))
            return 0;

        int count = 0;
        foreach (var c in value)
        {
            if (c == character)
                count++;
        }

        return count;
    }

    public static int CountOccurrences(string value, string substring)
    {
        if (string.IsNullOrEmpty(value) || string.IsNullOrEmpty(substring))
            return 0;

        int count = 0;
        int index = 0;

        while ((index = value.IndexOf(substring, index)) != -1)
        {
            count++;
            index += substring.Length;
        }

        return count;
    }

    public static string RemoveDuplicateSpaces(string value)
    {
        if (string.IsNullOrEmpty(value))
            return value ?? string.Empty;

        return Regex.Replace(value, @"\s+", " ").Trim();
    }

    public static string RemoveExtraWhitespace(string value)
    {
        return RemoveDuplicateSpaces(value);
    }

    public static string ToTitleCase(string value)
    {
        if (string.IsNullOrEmpty(value))
            return value ?? string.Empty;

        var words = value.Split(' ');
        var result = new StringBuilder();

        for (int i = 0; i < words.Length; i++)
        {
            if (i > 0)
                result.Append(' ');

            result.Append(FirstCharToUpper(words[i].ToLower()));
        }

        return result.ToString();
    }

    public static string ToSentenceCase(string value)
    {
        if (string.IsNullOrEmpty(value))
            return value ?? string.Empty;

        if (value.Length == 1)
            return value.ToUpper();

        return char.ToUpper(value[0]) + value.Substring(1).ToLower();
    }

    public static string EnsurePrefix(string value, string prefix)
    {
        if (string.IsNullOrEmpty(value))
            return prefix;

        if (string.IsNullOrEmpty(prefix))
            return value;

        if (value.StartsWith(prefix, StringComparison.Ordinal))
            return value;

        return prefix + value;
    }

    public static string EnsureSuffix(string value, string suffix)
    {
        if (string.IsNullOrEmpty(value))
            return suffix;

        if (string.IsNullOrEmpty(suffix))
            return value;

        if (value.EndsWith(suffix, StringComparison.Ordinal))
            return value;

        return value + suffix;
    }

    public static string RemovePrefix(string value, string prefix)
    {
        if (string.IsNullOrEmpty(value) || string.IsNullOrEmpty(prefix))
            return value ?? string.Empty;

        if (value.StartsWith(prefix, StringComparison.Ordinal))
            return value.Substring(prefix.Length);

        return value;
    }

    public static string RemoveSuffix(string value, string suffix)
    {
        if (string.IsNullOrEmpty(value) || string.IsNullOrEmpty(suffix))
            return value ?? string.Empty;

        if (value.EndsWith(suffix, StringComparison.Ordinal))
            return value.Substring(0, value.Length - suffix.Length);

        return value;
    }

    public static string RemovePrefixIgnoreCase(string value, string prefix)
    {
        if (string.IsNullOrEmpty(value) || string.IsNullOrEmpty(prefix))
            return value ?? string.Empty;

        if (value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            return value.Substring(prefix.Length);

        return value;
    }

    public static string RemoveSuffixIgnoreCase(string value, string suffix)
    {
        if (string.IsNullOrEmpty(value) || string.IsNullOrEmpty(suffix))
            return value ?? string.Empty;

        if (value.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
            return value.Substring(0, value.Length - suffix.Length);

        return value;
    }

    public static string SubstringBefore(string value, string delimiter)
    {
        if (string.IsNullOrEmpty(value) || string.IsNullOrEmpty(delimiter))
            return value ?? string.Empty;

        var index = value.IndexOf(delimiter, StringComparison.Ordinal);
        return index >= 0 ? value.Substring(0, index) : value;
    }

    public static string SubstringAfter(string value, string delimiter)
    {
        if (string.IsNullOrEmpty(value) || string.IsNullOrEmpty(delimiter))
            return value ?? string.Empty;

        var index = value.IndexOf(delimiter, StringComparison.Ordinal);
        return index >= 0 ? value.Substring(index + delimiter.Length) : string.Empty;
    }

    public static string SubstringBetween(string value, string startDelimiter, string endDelimiter)
    {
        if (string.IsNullOrEmpty(value) || string.IsNullOrEmpty(startDelimiter) || string.IsNullOrEmpty(endDelimiter))
            return string.Empty;

        var startIndex = value.IndexOf(startDelimiter, StringComparison.Ordinal);
        if (startIndex < 0)
            return string.Empty;

        startIndex += startDelimiter.Length;

        var endIndex = value.IndexOf(endDelimiter, startIndex, StringComparison.Ordinal);
        if (endIndex < 0)
            return string.Empty;

        return value.Substring(startIndex, endIndex - startIndex);
    }

    public static string LimitLength(string value, int maxLength, string suffix = "...")
    {
        return Truncate(value, maxLength, suffix);
    }
}