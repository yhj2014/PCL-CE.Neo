using System.Collections.Concurrent;

namespace PCL_CE.Neo.Core.Configuration;

public class ConfigValueCache<TValue>
{
    private readonly ConcurrentDictionary<object?, TValue> _cache = new();

    public bool TryRead(out TValue value, object? argument = null)
    {
        return _cache.TryGetValue(argument, out value);
    }

    public void Write(TValue value, object? argument = null)
    {
        _cache[argument] = value;
    }

    public void Invalidate(object? argument = null)
    {
        _cache.TryRemove(argument, out _);
    }

    public void InvalidateAll()
    {
        _cache.Clear();
    }
}