namespace PCL_CE.Neo.Core.Utils.Config;

public static class ConfigManager
{
    private static IConfigProvider? _defaultProvider;

    public static void Initialize(string configPath)
    {
        _defaultProvider = new JsonConfigProvider(configPath);
    }

    public static void Initialize(IConfigProvider provider)
    {
        _defaultProvider = provider;
    }

    public static T Get<T>(string key, T defaultValue = default!)
    {
        ThrowIfNotInitialized();
        return _defaultProvider!.Get(key, defaultValue);
    }

    public static bool TryGet<T>(string key, out T value)
    {
        ThrowIfNotInitialized();
        return _defaultProvider!.TryGet(key, out value);
    }

    public static void Set<T>(string key, T value)
    {
        ThrowIfNotInitialized();
        _defaultProvider!.Set(key, value);
    }

    public static bool ContainsKey(string key)
    {
        ThrowIfNotInitialized();
        return _defaultProvider!.ContainsKey(key);
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

    public static void Save()
    {
        ThrowIfNotInitialized();
        _defaultProvider!.Save();
    }

    public static void Load()
    {
        ThrowIfNotInitialized();
        _defaultProvider!.Load();
    }

    public static IDictionary<string, object> GetAll()
    {
        ThrowIfNotInitialized();
        return _defaultProvider!.GetAll();
    }

    private static void ThrowIfNotInitialized()
    {
        if (_defaultProvider == null)
            throw new InvalidOperationException("ConfigManager 未初始化，请先调用 Initialize 方法。");
    }
}