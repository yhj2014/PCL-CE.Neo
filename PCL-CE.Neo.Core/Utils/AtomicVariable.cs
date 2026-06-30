using System;
using System.Threading;

namespace PCL_CE.Neo.Core.Utils;

public class AtomicVariable<T> where T : class
{
    private T _value;

    public AtomicVariable(T value)
    {
        _value = value ?? throw new ArgumentNullException(nameof(value));
    }

    public T Value
    {
        get => Volatile.Read(ref _value);
        set => Volatile.Write(ref _value, value ?? throw new ArgumentNullException(nameof(value)));
    }

    public bool CompareAndSet(T expected, T newValue)
    {
        return Interlocked.CompareExchange(ref _value, newValue, expected) == expected;
    }

    public T GetAndSet(T newValue)
    {
        return Interlocked.Exchange(ref _value, newValue ?? throw new ArgumentNullException(nameof(newValue)));
    }

    public override string? ToString()
    {
        return Value?.ToString();
    }
}