namespace PCL_CE.Neo.Core.Utils.Caching;

public interface ICacheProvider
{
    T Get<T>(string key);
    T Get<T>(string key, T defaultValue);
    bool TryGet<T>(string key, out T value);
    void Set<T>(string key, T value, TimeSpan? expiration = null);
    void Set<T>(string key, T value, DateTime absoluteExpiration);
    bool Contains(string key);
    void Remove(string key);
    void Clear();
    long GetCount();
}