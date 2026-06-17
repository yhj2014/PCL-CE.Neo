using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace PCL_CE.Neo.Core.Utils.Exts;

public static class StringConvertExtension
{
    public static object? Convert(string? value, Type targetType)
    {
        ArgumentNullException.ThrowIfNull(targetType);

        if (targetType == typeof(string)) return value;

        if (value is null)
        {
            if (!targetType.IsValueType || Nullable.GetUnderlyingType(targetType) != null) return null;
            return Activator.CreateInstance(targetType);
        }

        var converter = TypeDescriptor.GetConverter(targetType);

        if (converter.CanConvertFrom(typeof(string)))
        {
            return converter.ConvertFromInvariantString(value);
        }

        if (typeof(IConvertible).IsAssignableFrom(targetType))
        {
            return System.Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);
        }

        if (targetType.IsEnum) return Enum.Parse(targetType, value, ignoreCase: true);

        var parse = targetType.GetMethod("Parse",
            BindingFlags.Public | BindingFlags.Static,
            binder: null, types: [typeof(string)], modifiers: null);
        if (parse is not null) return parse.Invoke(null, [value]);

        throw new NotSupportedException($"无法将字符串转换为类型 {targetType.FullName}");
    }

    public static T? Convert<T>(this string? value)
    {
        var obj = Convert(value, typeof(T));
        if (obj is null) return default;
        return (T)obj;
    }
}

public static class StringExtension
{
    public static string? ConvertToString(object? obj)
    {
        if (obj == null) return null;
        if (obj is string s) return s;

        var converter = TypeDescriptor.GetConverter(obj.GetType());
        if (converter.CanConvertTo(typeof(string)))
        {
            object? o = converter.ConvertToInvariantString(obj);
            return o as string;
        }

        if (obj is IFormattable fmt) return fmt.ToString(null, CultureInfo.InvariantCulture);

        return obj.ToString();
    }

    public static string? ConvertToString<T>(this T? value) => ConvertToString((object?)value);

    private static readonly char[] _B36Map = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();

    public static string FromB10ToB36(this string input)
    {
        var n = BigInteger.Parse(input);
        var s = new List<char>();
        while (n > 0)
        {
            var i = (n % 36).ToByteArray()[0];
            s.Add(_B36Map[i]);
            n /= 36;
        }
        s.Reverse();
        return string.Join("", s);
    }

    public static string FromB36ToB10(this string input)
    {
        var ns = input.Select(c => (c is >= '0' and <= '9') ? c - '0' : c - 'A' + 10).ToArray();
        var nb = ns.Aggregate(new BigInteger(0), (n, i) => n * 36 + i);
        return nb.ToString();
    }

    private static readonly char[] _B32Map = "23456789ABCDEFGHJKLMNPQRSTUVWXYZ".ToCharArray();

    public static string FromB10ToB32(this string input)
    {
        var n = BigInteger.Parse(input);
        var s = new List<char>();
        while (n > 0)
        {
            var i = (n % 32).ToByteArray()[0];
            s.Add(_B32Map[i]);
            n /= 32;
        }
        s.Reverse();
        return string.Join("", s);
    }

    public static string FromB32ToB10(this string input)
    {
        var ns = input.Select(Parse).ToArray();
        var nb = ns.Aggregate(new BigInteger(0), (n, i) => n * 32 + i);
        return nb.ToString();

        int Parse(char c) => c switch
        {
            >= '2' and <= '9' => c - '2',
            >= 'A' and <= 'H' => c - 'A' + 8,
            >= 'J' and <= 'N' => c - 'J' + 16,
            >= 'P' and <= 'Z' => c - 'P' + 21,
            _ => throw new ArgumentOutOfRangeException(nameof(input), $"Character '{c}' out of Base32 range")
        };
    }

    public static string FromB64ToB64UrlSafe(this string input) => input.Replace("+", "-").Replace("/", "_");
    public static string FromB64UrlSafeToB64(this string input) => input.Replace("-", "+").Replace("_", "/");

    public static byte[] FromB64ToBytes(this string input)
    {
        switch (input.Length % 4)
        {
            case 3:
                input += "===";
                break;
            case 2:
                input += "==";
                break;
            case 1:
                input += "=";
                break;
        }

        return Convert.FromBase64String(input);
    }

    public static byte[] FromB64UrlSafeToBytes(this string input) => input.FromB64UrlSafeToB64().FromB64ToBytes();

    public static T ParseToEnum<T>(this string input) where T : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return (T)(object)0;
        }
        else if (int.TryParse(input, out int numericValue))
        {
            return (T)(object)numericValue;
        }
        else
        {
            return Enum.Parse<T>(input, true);
        }
    }

    public static bool IsNullOrEmpty([NotNullWhen(false)] this string? value) => string.IsNullOrEmpty(value);

    public static bool IsNullOrWhiteSpace([NotNullWhen(false)] this string? value) => string.IsNullOrWhiteSpace(value);

    public static string ReplaceNullOrEmpty(this string? input, string? replacement = null)
        => string.IsNullOrEmpty(input) ? (replacement ?? string.Empty) : input;

    public static string ReplaceLineBreak(this string? input, string replacement = " ")
        => input?.Replace(RegexPatterns.NewLine, replacement) ?? string.Empty;

    [return: NotNullIfNotNull(nameof(input))]
    public static string? Replace(this string? input, Regex regex, string replacement)
        => input == null ? null : regex.Replace(input, replacement);

    public static bool IsMatch(this string? input, Regex regex)
        => input != null && regex.IsMatch(input);

    public static List<string> RegexSearch(this string str, Regex regex)
    {
        var result = new List<string>();
        var regexSearchRes = regex.Matches(str);
        if (regexSearchRes.Count == 0) return result;
        result.AddRange(from Match item in regexSearchRes select item.Value);
        return result;
    }

    public static bool IsASCII(this string str)
    {
        return str.All(c => c < 128);
    }

    public static bool StartsWithF(this string str, string prefix, bool ignoreCase = false)
        => str.StartsWith(prefix, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);

    public static bool EndsWithF(this string str, string suffix, bool ignoreCase = false)
        => str.EndsWith(suffix, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);

    public static bool ContainsF(this string str, string subStr, bool ignoreCase = false)
        => str.Contains(subStr, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);

    public static int IndexOfF(this string str, string subStr, bool ignoreCase = false)
        => str.IndexOf(subStr, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);

    public static int IndexOfF(this string str, string subStr, int startIndex, bool ignoreCase = false)
        => str.IndexOf(subStr, startIndex, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);

    public static int LastIndexOfF(this string str, string subStr, bool ignoreCase = false)
        => str.LastIndexOf(subStr, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);

    public static int LastIndexOfF(this string str, string subStr, int startIndex, bool ignoreCase = false)
        => str.LastIndexOf(subStr, startIndex, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);

    public static string NormalizeLineBreak(this string str) => str.ReplaceLineBreak("\r\n");
}