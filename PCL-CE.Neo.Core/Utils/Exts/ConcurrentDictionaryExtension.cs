using System.Collections.Concurrent;

namespace PCL_CE.Neo.Core.Utils.Exts;

public static class ConcurrentDictionaryExtension
{
    public static TValue GetOrAddSafe<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dictionary, TKey key, Func<TKey, TValue> valueFactory)
        where TKey : notnull
    {
        return dictionary.GetOrAdd(key, valueFactory);
    }

    public static TValue GetOrAddSafe<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dictionary, TKey key, TValue value)
        where TKey : notnull
    {
        return dictionary.GetOrAdd(key, value);
    }

    public static bool TryRemoveSafe<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dictionary, TKey key)
        where TKey : notnull
    {
        return dictionary.TryRemove(key, out _);
    }

    public static bool TryGetOrAdd<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dictionary, TKey key, Func<TKey, TValue> valueFactory, out TValue value)
        where TKey : notnull
    {
        if (dictionary.TryGetValue(key, out value))
            return true;

        value = dictionary.GetOrAdd(key, valueFactory);
        return false;
    }

    public static void ClearSafe<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dictionary)
        where TKey : notnull
    {
        dictionary.Clear();
    }

    public static int CountSafe<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dictionary)
        where TKey : notnull
    {
        return dictionary.Count;
    }
}