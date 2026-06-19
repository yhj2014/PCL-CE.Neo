using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace PCL_CE.Neo.Core.Utils;

public class ConcurrentSet<T> : IReadOnlyCollection<T>, ICollection<T> where T : notnull
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
}