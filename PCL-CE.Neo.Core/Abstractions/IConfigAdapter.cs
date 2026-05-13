namespace PCL_CE.Neo.Core.Abstractions;

public interface IConfigAdapter
{
    T GetConfig<T>(string key, T defaultValue = default!);
    void SetConfig<T>(string key, T value);
    T GetInstanceConfig<T>(string instanceId, string key, T defaultValue = default!);
    void SetInstanceConfig<T>(string instanceId, string key, T value);
    bool HasConfig(string key);
    void SaveConfig();
    void LoadConfig();
}
