using PCL_CE.Neo.Core.Configuration;

namespace PCL_CE.Neo.Core.Configuration.Storage;

public abstract class ConfigStorage : IConfigProvider
{
    public abstract bool GetValue<T>(string key, out T value, object? argument = null);
    public abstract void SetValue<T>(string key, T value, object? argument = null);
    public abstract bool Exists(string key, object? argument = null);
    public abstract void Delete(string key, object? argument = null);
    public virtual void Stop() { }
}