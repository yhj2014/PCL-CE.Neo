namespace PCL_CE.Neo.Core.Utils.Caching;

public static class CacheManager
{
    private static ICacheProvider? _defaultProvider;

    public static void Initialize(ICacheProvider provider)
    {
        _defaultProvider = provider;
    }

    public static void Initialize()
    {
        _defaultProvider = new MemoryCacheProvider();
    }

    public static T Get<T>(string key)
    {
        ThrowIfNotInitialized();
        return _defaultProvider!.Get<T>(key);
    }

    public static T Get<T>(string key, T defaultValue)
    {
        ThrowIfNotInitialized();
        return _defaultProvider!.Get(key, defaultValue);
    }

    public static bool TryGet<T>(string key, out T value)
    {
        ThrowIfNotInitialized();
        return _defaultProvider!.TryGet(key, out value);
    }

    public static void Set<T>(string key, T value, TimeSpan? expiration = null)
    {
        ThrowIfNotInitialized();
        _defaultProvider!.Set(key, value, expiration);
    }

    public static void Set<T>(string key, T value, DateTime absoluteExpiration)
    {
        ThrowIfNotInitialized();
        _defaultProvider!.Set(key, value, absoluteExpiration);
    }

    public static bool Contains(string key)
    {
        ThrowIfNotInitialized();
        return _defaultProvider!.Contains(key);
    }

    public static void Remove(string key)
    {
        ThrowIfNotInitialized();
        _defaultProvider!.Remove(key);
    }

    public static void Clear()
    {
        ThrowIfNotInitialized();
        _defaultProvider!.Clear();
    }

    public static long GetCount()
    {
        ThrowIfNotInitialized();
        return _defaultProvider!.GetCount();
    }

    private static void ThrowIfNotInitialized()
    {
        if (_defaultProvider == null)
            throw new InvalidOperationException("CacheManager 未初始化，请先调用 Initialize 方法。");
    }
}