using System.Threading;

namespace PCL_CE.Neo.Core.Utils;

public class AtomicVariable<T> where T : class?
{
    private T _value;

    public AtomicVariable(T value)
    {
        _value = value;
    }

    public T Value
    {
        get => Volatile.Read(ref _value);
        set => Volatile.Write(ref _value, value);
    }

    public T GetAndSet(T newValue)
    {
        return Interlocked.Exchange(ref _value, newValue);
    }

    public bool CompareAndSet(T expected, T newValue)
    {
        return Interlocked.CompareExchange(ref _value, newValue, expected) == expected;
    }

    public override string ToString()
    {
        return Value?.ToString() ?? string.Empty;
    }

    public static implicit operator T(AtomicVariable<T> variable)
    {
        return variable.Value;
    }
}

public class AtomicInt32
{
    private int _value;

    public AtomicInt32(int value = 0)
    {
        _value = value;
    }

    public int Value
    {
        get => Volatile.Read(ref _value);
        set => Volatile.Write(ref _value, value);
    }

    public int Increment()
    {
        return Interlocked.Increment(ref _value);
    }

    public int Decrement()
    {
        return Interlocked.Decrement(ref _value);
    }

    public int Add(int value)
    {
        return Interlocked.Add(ref _value, value);
    }

    public int GetAndSet(int newValue)
    {
        return Interlocked.Exchange(ref _value, newValue);
    }

    public bool CompareAndSet(int expected, int newValue)
    {
        return Interlocked.CompareExchange(ref _value, newValue, expected) == expected;
    }

    public override string ToString()
    {
        return Value.ToString();
    }

    public static implicit operator int(AtomicInt32 variable)
    {
        return variable.Value;
    }
}

public class AtomicInt64
{
    private long _value;

    public AtomicInt64(long value = 0)
    {
        _value = value;
    }

    public long Value
    {
        get => Volatile.Read(ref _value);
        set => Volatile.Write(ref _value, value);
    }

    public long Increment()
    {
        return Interlocked.Increment(ref _value);
    }

    public long Decrement()
    {
        return Interlocked.Decrement(ref _value);
    }

    public long Add(long value)
    {
        return Interlocked.Add(ref _value, value);
    }

    public long GetAndSet(long newValue)
    {
        return Interlocked.Exchange(ref _value, newValue);
    }

    public bool CompareAndSet(long expected, long newValue)
    {
        return Interlocked.CompareExchange(ref _value, newValue, expected) == expected;
    }

    public override string ToString()
    {
        return Value.ToString();
    }

    public static implicit operator long(AtomicInt64 variable)
    {
        return variable.Value;
    }
}

public class AtomicBoolean
{
    private int _value;

    public AtomicBoolean(bool value = false)
    {
        _value = value ? 1 : 0;
    }

    public bool Value
    {
        get => Volatile.Read(ref _value) == 1;
        set => Volatile.Write(ref _value, value ? 1 : 0);
    }

    public bool GetAndSet(bool newValue)
    {
        var newIntValue = newValue ? 1 : 0;
        var oldIntValue = Interlocked.Exchange(ref _value, newIntValue);
        return oldIntValue == 1;
    }

    public bool CompareAndSet(bool expected, bool newValue)
    {
        var expectedIntValue = expected ? 1 : 0;
        var newIntValue = newValue ? 1 : 0;
        return Interlocked.CompareExchange(ref _value, newIntValue, expectedIntValue) == expectedIntValue;
    }

    public override string ToString()
    {
        return Value.ToString();
    }

    public static implicit operator bool(AtomicBoolean variable)
    {
        return variable.Value;
    }
}