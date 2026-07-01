using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace PCL_CE.Neo.Core.Utils;

public class ConcurrentSet<T> : ICollection<T>, IReadOnlyCollection<T> where T : notnull
{
    private readonly ConcurrentDictionary<T, byte> _dictionary;

    public ConcurrentSet()
    {
        _dictionary = new ConcurrentDictionary<T, byte>();
    }

    public ConcurrentSet(IEnumerable<T> collection)
    {
        _dictionary = new ConcurrentDictionary<T, byte>();
        foreach (var item in collection)
        {
            _dictionary.TryAdd(item, 0);
        }
    }

    public int Count => _dictionary.Count;

    public bool IsReadOnly => false;

    public bool Add(T item)
    {
        return _dictionary.TryAdd(item, 0);
    }

    void ICollection<T>.Add(T item)
    {
        _dictionary.TryAdd(item, 0);
    }

    public bool Contains(T item)
    {
        return _dictionary.ContainsKey(item);
    }

    public bool Remove(T item)
    {
        return _dictionary.TryRemove(item, out _);
    }

    public void Clear()
    {
        _dictionary.Clear();
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        _dictionary.Keys.CopyTo(array, arrayIndex);
    }

    public IEnumerator<T> GetEnumerator()
    {
        return _dictionary.Keys.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public bool TryGetValue(T equalValue, out T actualValue)
    {
        if (_dictionary.TryGetValue(equalValue, out _))
        {
            actualValue = equalValue;
            return true;
        }

        foreach (var key in _dictionary.Keys)
        {
            if (Equals(key, equalValue))
            {
                actualValue = key;
                return true;
            }
        }

        actualValue = default!;
        return false;
    }

    public void UnionWith(IEnumerable<T> other)
    {
        foreach (var item in other)
        {
            _dictionary.TryAdd(item, 0);
        }
    }

    public void ExceptWith(IEnumerable<T> other)
    {
        foreach (var item in other)
        {
            _dictionary.TryRemove(item, out _);
        }
    }

    public void IntersectWith(IEnumerable<T> other)
    {
        var otherSet = new HashSet<T>(other);
        foreach (var item in _dictionary.Keys.ToArray())
        {
            if (!otherSet.Contains(item))
            {
                _dictionary.TryRemove(item, out _);
            }
        }
    }

    public bool IsSubsetOf(IEnumerable<T> other)
    {
        var otherSet = new HashSet<T>(other);
        return _dictionary.Keys.All(otherSet.Contains);
    }

    public bool IsSupersetOf(IEnumerable<T> other)
    {
        return other.All(_dictionary.ContainsKey);
    }
}