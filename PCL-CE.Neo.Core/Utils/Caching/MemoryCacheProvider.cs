using System.Collections.Concurrent;

namespace PCL_CE.Neo.Core.Utils.Caching;

public class MemoryCacheProvider : ICacheProvider
{
    private readonly ConcurrentDictionary<string, CacheItem> _cache = new ConcurrentDictionary<string, CacheItem>();
    private readonly TimeSpan _defaultExpiration = TimeSpan.FromMinutes(30);

    private class CacheItem
    {
        public object Value { get; set; } = null!;
        public DateTime ExpirationTime { get; set; }
        public bool IsExpired => DateTime.Now >= ExpirationTime;
    }

    public T Get<T>(string key)
    {
        return Get<T>(key, default!);
    }

    public T Get<T>(string key, T defaultValue)
    {
        if (TryGet<T>(key, out T value))
            return value;
        return defaultValue;
    }

    public bool TryGet<T>(string key, out T value)
    {
        value = default!;

        if (!_cache.TryGetValue(key, out CacheItem? item))
            return false;

        if (item.IsExpired)
        {
            _cache.TryRemove(key, out _);
            return false;
        }

        if (item.Value is T typedValue)
        {
            value = typedValue;
            return true;
        }

        return false;
    }

    public void Set<T>(string key, T value, TimeSpan? expiration = null)
    {
        DateTime expirationTime = expiration.HasValue
            ? DateTime.Now.Add(expiration.Value)
            : DateTime.Now.Add(_defaultExpiration);

        _cache[key] = new CacheItem
        {
            Value = value ?? throw new ArgumentNullException(nameof(value)),
            ExpirationTime = expirationTime
        };
    }

    public void Set<T>(string key, T value, DateTime absoluteExpiration)
    {
        _cache[key] = new CacheItem
        {
            Value = value ?? throw new ArgumentNullException(nameof(value)),
            ExpirationTime = absoluteExpiration
        };
    }

    public bool Contains(string key)
    {
        if (!_cache.TryGetValue(key, out CacheItem? item))
            return false;

        if (item.IsExpired)
        {
            _cache.TryRemove(key, out _);
            return false;
        }

        return true;
    }

    public void Remove(string key)
    {
        _cache.TryRemove(key, out _);
    }

    public void Clear()
    {
        _cache.Clear();
    }

    public long GetCount()
    {
        CleanupExpired();
        return _cache.Count;
    }

    public void CleanupExpired()
    {
        foreach (var (key, item) in _cache)
        {
            if (item.IsExpired)
                _cache.TryRemove(key, out _);
        }
    }
}