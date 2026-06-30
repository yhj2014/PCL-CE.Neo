using System.Collections.Concurrent;

namespace PCL_CE.Neo.Core.Configuration;

public sealed class ConfigValueCache<TValue>
{
    private readonly ConcurrentDictionary<object?, TValue> _cache = new();
    private TValue? _defaultValue;
    private bool _hasDefaultValue;

    public bool TryRead(out TValue? value, object? argument = null)
    {
        if (argument == null)
        {
            value = _hasDefaultValue ? _defaultValue : default;
            return _hasDefaultValue;
        }
        return _cache.TryGetValue(argument, out value);
    }

    public void Write(TValue value, object? argument = null)
    {
        if (argument == null)
        {
            _defaultValue = value;
            _hasDefaultValue = true;
        }
        else
        {
            _cache[argument] = value;
        }
    }

    public void Invalidate(object? argument = null)
    {
        if (argument == null)
        {
            _hasDefaultValue = false;
            _defaultValue = default;
        }
        else
        {
            _cache.TryRemove(argument, out _);
        }
    }

    public void InvalidateAll()
    {
        _hasDefaultValue = false;
        _defaultValue = default;
        _cache.Clear();
    }
}