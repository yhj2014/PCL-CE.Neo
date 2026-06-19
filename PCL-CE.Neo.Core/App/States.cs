using System.Collections.Concurrent;

namespace PCL_CE.Neo.Core.App;

public static class States
{
    private static readonly ConcurrentDictionary<string, object?> _state = new();

    public static T? Get<T>(string key)
    {
        if (_state.TryGetValue(key, out var value))
        {
            return value is T t ? t : default;
        }
        return default;
    }

    public static object? Get(string key)
    {
        _state.TryGetValue(key, out var value);
        return value;
    }

    public static void Set<T>(string key, T? value)
    {
        if (value == null)
        {
            _state.TryRemove(key, out _);
        }
        else
        {
            _state[key] = value;
        }
    }

    public static bool Contains(string key)
    {
        return _state.ContainsKey(key);
    }

    public static bool Remove(string key)
    {
        return _state.TryRemove(key, out _);
    }

    public static void Clear()
    {
        _state.Clear();
    }

    public static int Count => _state.Count;

    public static IEnumerable<string> Keys => _state.Keys;
}