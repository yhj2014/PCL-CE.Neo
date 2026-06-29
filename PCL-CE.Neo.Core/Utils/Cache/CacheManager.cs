using System;
using System.Collections.Concurrent;
using System.Timers;

namespace PCL_CE.Neo.Core.Utils.Cache;

public interface ICacheManager
{
    T? Get<T>(string key);
    void Set<T>(string key, T value, TimeSpan? expiry = null);
    bool Remove(string key);
    bool Exists(string key);
    void Clear();
    long Count { get; }
}

public class CacheManager : ICacheManager
{
    private readonly ConcurrentDictionary<string, CacheItem> _cache = new();
    private readonly Timer _cleanupTimer;

    public CacheManager()
    {
        _cleanupTimer = new Timer(TimeSpan.FromMinutes(5).TotalMilliseconds);
        _cleanupTimer.Elapsed += CleanupExpiredItems;
        _cleanupTimer.Start();
    }

    public T? Get<T>(string key)
    {
        try
        {
            if (_cache.TryGetValue(key, out var item))
            {
                if (item.IsExpired)
                {
                    _cache.TryRemove(key, out _);
                    return default;
                }
                
                item.LastAccessed = DateTime.Now;
                return (T?)item.Value;
            }
            
            return default;
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, $"Failed to get cache item: {key}");
            return default;
        }
    }

    public void Set<T>(string key, T value, TimeSpan? expiry = null)
    {
        try
        {
            var item = new CacheItem(value, expiry);
            _cache[key] = item;
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, $"Failed to set cache item: {key}");
        }
    }

    public bool Remove(string key)
    {
        try
        {
            return _cache.TryRemove(key, out _);
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, $"Failed to remove cache item: {key}");
            return false;
        }
    }

    public bool Exists(string key)
    {
        try
        {
            if (_cache.TryGetValue(key, out var item))
            {
                if (item.IsExpired)
                {
                    _cache.TryRemove(key, out _);
                    return false;
                }
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, $"Failed to check cache item existence: {key}");
            return false;
        }
    }

    public void Clear()
    {
        try
        {
            _cache.Clear();
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, "Failed to clear cache");
        }
    }

    public long Count => _cache.Count;

    private void CleanupExpiredItems(object? sender, ElapsedEventArgs e)
    {
        try
        {
            foreach (var pair in _cache)
            {
                if (pair.Value.IsExpired)
                {
                    _cache.TryRemove(pair.Key, out _);
                }
            }
        }
        catch (Exception ex)
        {
            LogWrapper.Warn(ex, "Failed to cleanup expired cache items");
        }
    }

    private class CacheItem
    {
        public object Value { get; }
        public DateTime Created { get; }
        public DateTime LastAccessed { get; set; }
        public TimeSpan? Expiry { get; }

        public bool IsExpired => Expiry.HasValue && DateTime.Now > Created + Expiry.Value;

        public CacheItem(object value, TimeSpan? expiry)
        {
            Value = value;
            Created = DateTime.Now;
            LastAccessed = DateTime.Now;
            Expiry = expiry;
        }
    }
}