using System.Collections.Concurrent;

namespace PCL_CE.Neo.Core.Utils.Exts;

public static class ConcurrentDictionaryExtension
{
    public static TValue GetOrAdd<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dict, TKey key, TValue value) =>
        dict.GetOrAdd(key, value);

    public static TValue GetOrAdd<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dict, TKey key, Func<TKey, TValue> valueFactory) =>
        dict.GetOrAdd(key, valueFactory);

    public static bool TryUpdate<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dict, TKey key, TValue newValue, TValue comparisonValue) =>
        dict.TryUpdate(key, newValue, comparisonValue);

    public static bool Remove<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dict, TKey key, out TValue value) =>
        dict.TryRemove(key, out value);

    public static int Count<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dict) =>
        dict.Count;
}