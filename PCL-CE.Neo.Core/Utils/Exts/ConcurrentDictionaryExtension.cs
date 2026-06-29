using System;
using System.Collections.Concurrent;

namespace PCL.CE.Neo.Core.Utils.Exts;

public static class ConcurrentDictionaryExtension
{
    public static TValue GetOrAdd<TKey, TValue>(
        this ConcurrentDictionary<TKey, TValue> dictionary,
        TKey key,
        Func<TKey, TValue> valueFactory)
    {
        return dictionary.GetOrAdd(key, valueFactory);
    }

    public static TValue GetOrAdd<TKey, TValue>(
        this ConcurrentDictionary<TKey, TValue> dictionary,
        TKey key,
        Func<TValue> valueFactory)
    {
        return dictionary.GetOrAdd(key, _ => valueFactory());
    }

    public static bool TryGetValueOrDefault<TKey, TValue>(
        this ConcurrentDictionary<TKey, TValue> dictionary,
        TKey key,
        out TValue value)
    {
        if (dictionary.TryGetValue(key, out value))
        {
            return true;
        }
        value = default!;
        return false;
    }

    public static TValue GetValueOrDefault<TKey, TValue>(
        this ConcurrentDictionary<TKey, TValue> dictionary,
        TKey key,
        TValue defaultValue = default!)
    {
        if (dictionary.TryGetValue(key, out var value))
        {
            return value;
        }
        return defaultValue;
    }

    public static bool RemoveIf<TKey, TValue>(
        this ConcurrentDictionary<TKey, TValue> dictionary,
        TKey key,
        Predicate<TValue> predicate)
    {
        if (dictionary.TryGetValue(key, out var value) && predicate(value))
        {
            return dictionary.TryRemove(key, out _);
        }
        return false;
    }
}