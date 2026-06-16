using System.Collections.Generic;

namespace PCL_CE.Neo.Core.Configuration.Storage;

public interface IKeyValueFileProvider
{
    string Path { get; }
    bool TryGet(string key, out object? value);
    void Set(string key, object? value);
    void Delete(string key);
    bool Exists(string key);
    List<KeyValuePair> GetAll();
    void Sync();
}