namespace PCL_CE.Neo.Core.Configuration;

public interface IConfigProvider
{
    bool GetValue<T>(string key, out T value, object? argument = null);
    void SetValue<T>(string key, T value, object? argument = null);
    bool Exists(string key, object? argument = null);
    void Delete(string key, object? argument = null);
    void Stop();
}