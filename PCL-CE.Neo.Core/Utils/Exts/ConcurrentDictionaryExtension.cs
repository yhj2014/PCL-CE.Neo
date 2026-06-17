using System.Collections.Concurrent;

namespace PCL_CE.Neo.Core.Utils.Exts;

public static class ConcurrentDictionaryExtension
{
    public static TValue? GetValueOrDefault<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dictionary, TKey key)
        where TKey : notnull
    {
        dictionary.TryGetValue(key, out var value);
        return value;
    }
}