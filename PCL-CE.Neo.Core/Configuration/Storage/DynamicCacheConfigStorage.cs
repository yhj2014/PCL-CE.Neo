using System;
using System.Collections.Concurrent;

namespace PCL_CE.Neo.Core.Configuration.Storage;

public class DynamicCacheConfigStorage(ConfigStorage source) : ConfigStorage
{
    public ConfigStorage Source { get; } = source;
    
    private readonly ConcurrentDictionary<string, object?> _cache = new();

    public override bool GetValue<T>(string key, out T value, object? argument = null)
    {
        if (_cache.TryGetValue(key, out var cached))
        {
            if (cached is T typedValue)
            {
                value = typedValue;
                return true;
            }
        }

        if (Source.GetValue(key, out value, argument))
        {
            _cache.TryAdd(key, value);
            return true;
        }

        value = default!;
        return false;
    }

    public override void SetValue<T>(string key, T value, object? argument = null)
    {
        _cache[key] = value;
        Source.SetValue(key, value, argument);
    }

    public override bool Exists(string key, object? argument = null)
    {
        if (_cache.ContainsKey(key))
            return true;
        
        var exists = Source.Exists(key, argument);
        if (exists)
            _cache.TryAdd(key, null);
        
        return exists;
    }

    public override void Delete(string key, object? argument = null)
    {
        _cache.TryRemove(key, out _);
        Source.Delete(key, argument);
    }

    public void ClearCache()
    {
        _cache.Clear();
    }
}