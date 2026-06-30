using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace PCL_CE.Neo.Core.Minecraft;

public static class McFormatter
{
    private static readonly Regex _colorCodeRegex = new(@"§([0-9a-fk-or])", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public static string StripFormatting(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text ?? string.Empty;

        return _colorCodeRegex.Replace(text, string.Empty);
    }

    public static string StripColors(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text ?? string.Empty;

        return _colorCodeRegex.Replace(text, (match) =>
        {
            var code = match.Groups[1].Value.ToLowerInvariant();
            return "0123456789abcdef".Contains(code) ? string.Empty : match.Value;
        });
    }

    public static string StripStyles(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text ?? string.Empty;

        return _colorCodeRegex.Replace(text, (match) =>
        {
            var code = match.Groups[1].Value.ToLowerInvariant();
            return "klmnor".Contains(code) ? string.Empty : match.Value;
        });
    }

    public static string ApplyFormatting(string text, string formatCode)
    {
        if (string.IsNullOrEmpty(text))
            return text ?? string.Empty;

        if (string.IsNullOrEmpty(formatCode) || !IsValidFormatCode(formatCode))
            return text;

        return $"§{formatCode}{text}§r";
    }

    public static string Colorize(string text, string colorCode)
    {
        if (string.IsNullOrEmpty(text))
            return text ?? string.Empty;

        if (string.IsNullOrEmpty(colorCode) || !IsValidColorCode(colorCode))
            return text;

        return $"§{colorCode}{text}§r";
    }

    public static bool IsValidFormatCode(string code)
    {
        if (string.IsNullOrEmpty(code))
            return false;

        var normalized = code.ToLowerInvariant();
        return normalized.Length == 1 && "0123456789abcdefklmnor".Contains(normalized[0]);
    }

    public static bool IsValidColorCode(string code)
    {
        if (string.IsNullOrEmpty(code))
            return false;

        var normalized = code.ToLowerInvariant();
        return normalized.Length == 1 && "0123456789abcdef".Contains(normalized[0]);
    }

    public static bool IsValidStyleCode(string code)
    {
        if (string.IsNullOrEmpty(code))
            return false;

        var normalized = code.ToLowerInvariant();
        return normalized.Length == 1 && "klmnor".Contains(normalized[0]);
    }

    public static string GetColorName(char code)
    {
        return char.ToLowerInvariant(code) switch
        {
            '0' => "Black",
            '1' => "Dark Blue",
            '2' => "Dark Green",
            '3' => "Dark Aqua",
            '4' => "Dark Red",
            '5' => "Dark Purple",
            '6' => "Gold",
            '7' => "Gray",
            '8' => "Dark Gray",
            '9' => "Blue",
            'a' => "Green",
            'b' => "Aqua",
            'c' => "Red",
            'd' => "Light Purple",
            'e' => "Yellow",
            'f' => "White",
            _ => "Unknown"
        };
    }

    public static string GetStyleName(char code)
    {
        return char.ToLowerInvariant(code) switch
        {
            'k' => "Obfuscated",
            'l' => "Bold",
            'm' => "Strikethrough",
            'n' => "Underline",
            'o' => "Italic",
            'r' => "Reset",
            _ => "Unknown"
        };
    }

    public static IEnumerable<(string Text, string Format)> ParseFormattedText(string text)
    {
        if (string.IsNullOrEmpty(text))
            yield break;

        var currentFormat = string.Empty;
        var currentText = new StringBuilder();

        for (int i = 0; i < text.Length; i++)
        {
            if (text[i] == '§' && i + 1 < text.Length)
            {
                if (currentText.Length > 0)
                {
                    yield return (currentText.ToString(), currentFormat);
                    currentText.Clear();
                }

                var code = text[i + 1];
                if (code == 'r')
                {
                    currentFormat = string.Empty;
                }
                else if (IsValidFormatCode(code.ToString()))
                {
                    if (code == 'k' || code == 'l' || code == 'm' || code == 'n' || code == 'o')
                    {
                        currentFormat += $"§{code}";
                    }
                    else
                    {
                        currentFormat = $"§{code}";
                    }
                }

                i++;
            }
            else
            {
                currentText.Append(text[i]);
            }
        }

        if (currentText.Length > 0)
        {
            yield return (currentText.ToString(), currentFormat);
        }
    }

    public static string LimitLength(string text, int maxLength)
    {
        if (string.IsNullOrEmpty(text))
            return text ?? string.Empty;

        var stripped = StripFormatting(text);
        if (stripped.Length <= maxLength)
            return text;

        var result = new StringBuilder();
        var strippedLength = 0;

        for (int i = 0; i < text.Length; i++)
        {
            if (text[i] == '§' && i + 1 < text.Length)
            {
                result.Append(text[i]);
                result.Append(text[i + 1]);
                i++;
            }
            else
            {
                if (strippedLength >= maxLength)
                    break;

                result.Append(text[i]);
                strippedLength++;
            }
        }

        return result.ToString();
    }

    public static string ReplaceColorCodes(string text, Dictionary<string, string> replacements)
    {
        if (string.IsNullOrEmpty(text))
            return text ?? string.Empty;

        if (replacements == null || replacements.Count == 0)
            return text;

        var result = text;
        foreach (var replacement in replacements)
        {
            result = result.Replace(replacement.Key, replacement.Value);
        }
        return result;
    }

    public static string ToPlainText(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text ?? string.Empty;

        return _colorCodeRegex.Replace(text, string.Empty);
    }

    public static string EscapeFormatting(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text ?? string.Empty;

        return text.Replace("§", "§§");
    }

    public static string UnescapeFormatting(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text ?? string.Empty;

        return text.Replace("§§", "§");
    }
}