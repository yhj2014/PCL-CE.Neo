namespace PCL_CE.Neo.Core.Utils.Config;

public interface IConfigProvider
{
    T Get<T>(string key, T defaultValue = default!);
    bool TryGet<T>(string key, out T value);
    void Set<T>(string key, T value);
    bool ContainsKey(string key);
    void Remove(string key);
    void Clear();
    void Save();
    void Load();
    IDictionary<string, object> GetAll();
}