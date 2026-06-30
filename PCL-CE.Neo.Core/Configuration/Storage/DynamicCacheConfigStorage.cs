using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace PCL_CE.Neo.Core.Configuration.Storage;

public class DynamicCacheConfigStorage : ConfigStorage
{
    public Func<object?, ConfigStorage> StorageFactory { get; set; } = _ => throw new NotImplementedException();

    private readonly ConcurrentDictionary<object?, ConfigStorage> _storageCache = new();

    private ConfigStorage _GetStorage(object? argument)
    {
        return _storageCache.GetOrAdd(argument ?? string.Empty, StorageFactory);
    }

    protected override bool OnAccess<TKey, TValue>(
        StorageAction action,
        ref TKey key,
        [NotNullWhen(true)] ref TValue value,
        object? argument)
    {
        var storage = _GetStorage(argument);
        return storage.Access(action, ref key, ref value, argument);
    }

    protected override void OnStop()
    {
        foreach (var storage in _storageCache.Values)
        {
            storage.Stop();
        }
        _storageCache.Clear();
    }
}