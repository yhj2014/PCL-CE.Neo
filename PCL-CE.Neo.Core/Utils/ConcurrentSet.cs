using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;

namespace PCL.CE.Neo.Core.Utils;

public class ConcurrentSet<T> : ICollection<T>, IReadOnlyCollection<T>
{
    private readonly ConcurrentDictionary<T, byte> _dictionary;

    public ConcurrentSet()
    {
        _dictionary = new ConcurrentDictionary<T, byte>();
    }

    public ConcurrentSet(IEnumerable<T> items)
    {
        _dictionary = new ConcurrentDictionary<T, byte>();
        foreach (var item in items)
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
        Add(item);
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

    public T[] ToArray()
    {
        return _dictionary.Keys.ToArray();
    }

    public List<T> ToList()
    {
        return _dictionary.Keys.ToList();
    }

    public ReadOnlyCollection<T> AsReadOnly()
    {
        return new ReadOnlyCollection<T>(ToList());
    }

    public bool AddRange(IEnumerable<T> items)
    {
        var added = false;
        foreach (var item in items)
        {
            if (Add(item))
            {
                added = true;
            }
        }
        return added;
    }

    public bool RemoveRange(IEnumerable<T> items)
    {
        var removed = false;
        foreach (var item in items)
        {
            if (Remove(item))
            {
                removed = true;
            }
        }
        return removed;
    }

    public bool TryGetValue(T item, out T? value)
    {
        if (_dictionary.TryGetValue(item, out _))
        {
            value = item;
            return true;
        }
        value = default;
        return false;
    }

    public bool Replace(T oldValue, T newValue)
    {
        if (_dictionary.TryRemove(oldValue, out _))
        {
            return _dictionary.TryAdd(newValue, 0);
        }
        return false;
    }
}