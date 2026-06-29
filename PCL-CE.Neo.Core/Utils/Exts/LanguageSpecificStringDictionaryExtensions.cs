using System;
using System.Collections.Generic;

namespace PCL_CE.Neo.Core.Utils.Exts;

public static class LanguageSpecificStringDictionaryExtensions
{
    private static readonly Dictionary<string, string> FallbackLanguages = new()
    {
        { "zh-CN", "zh" },
        { "zh-TW", "zh" },
        { "zh-HK", "zh" },
        { "en-US", "en" },
        { "en-GB", "en" },
        { "en-AU", "en" },
        { "ja-JP", "ja" },
        { "ko-KR", "ko" },
        { "fr-FR", "fr" },
        { "de-DE", "de" },
        { "es-ES", "es" },
        { "ru-RU", "ru" }
    };

    public static string GetLocalizedString(this Dictionary<string, string> dictionary, string languageCode)
    {
        if (dictionary == null || dictionary.Count == 0)
            return string.Empty;

        if (dictionary.TryGetValue(languageCode, out var value))
            return value;

        if (FallbackLanguages.TryGetValue(languageCode, out var fallback))
        {
            if (dictionary.TryGetValue(fallback, out var fallbackValue))
                return fallbackValue;
        }

        var baseLang = languageCode.Split('-')[0];
        if (dictionary.TryGetValue(baseLang, out var baseValue))
            return baseValue;

        if (dictionary.TryGetValue("en", out var englishValue))
            return englishValue;

        return dictionary.Values.Count > 0 ? dictionary.Values.GetEnumerator().Current : string.Empty;
    }

    public static string GetLocalizedString(this Dictionary<string, string> dictionary, string languageCode, string defaultValue)
    {
        var result = dictionary.GetLocalizedString(languageCode);
        return string.IsNullOrEmpty(result) ? defaultValue : result;
    }

    public static Dictionary<string, string> MergeWith(this Dictionary<string, string> dictionary, Dictionary<string, string> other)
    {
        if (other == null)
            return dictionary;

        var result = new Dictionary<string, string>(dictionary);
        foreach (var pair in other)
        {
            result[pair.Key] = pair.Value;
        }
        return result;
    }

    public static bool HasLocalization(this Dictionary<string, string> dictionary, string languageCode)
    {
        if (dictionary == null)
            return false;

        if (dictionary.ContainsKey(languageCode))
            return true;

        if (FallbackLanguages.TryGetValue(languageCode, out var fallback))
        {
            if (dictionary.ContainsKey(fallback))
                return true;
        }

        var baseLang = languageCode.Split('-')[0];
        return dictionary.ContainsKey(baseLang);
    }

    public static Dictionary<string, string> FilterByLanguageFamily(this Dictionary<string, string> dictionary, string languageCode)
    {
        if (dictionary == null)
            return new Dictionary<string, string>();

        var result = new Dictionary<string, string>();
        var baseLang = languageCode.Split('-')[0];

        foreach (var pair in dictionary)
        {
            if (pair.Key == languageCode || pair.Key == baseLang)
            {
                result[pair.Key] = pair.Value;
            }
        }

        return result;
    }
}