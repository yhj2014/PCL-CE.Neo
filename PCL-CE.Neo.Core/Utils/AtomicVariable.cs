using System.Threading;

namespace PCL.CE.Neo.Core.Utils;

public class AtomicVariable<T> where T : class
{
    private T? _value;

    public T? Value
    {
        get => Volatile.Read(ref _value);
        set => Volatile.Write(ref _value, value);
    }

    public AtomicVariable(T? initialValue = null)
    {
        _value = initialValue;
    }

    public bool CompareAndSet(T? expected, T? newValue)
    {
        return Interlocked.CompareExchange(ref _value, newValue, expected) == expected;
    }

    public T? Exchange(T? newValue)
    {
        return Interlocked.Exchange(ref _value, newValue);
    }

    public T? GetAndSet(T? newValue)
    {
        return Interlocked.Exchange(ref _value, newValue);
    }
}

public class AtomicInt
{
    private int _value;

    public int Value
    {
        get => Volatile.Read(ref _value);
        set => Interlocked.Exchange(ref _value, value);
    }

    public AtomicInt(int initialValue = 0)
    {
        _value = initialValue;
    }

    public int Increment() => Interlocked.Increment(ref _value);
    
    public int Decrement() => Interlocked.Decrement(ref _value);
    
    public int Add(int value) => Interlocked.Add(ref _value, value);
    
    public bool CompareAndSet(int expected, int newValue) => Interlocked.CompareExchange(ref _value, newValue, expected) == expected;
    
    public int Exchange(int newValue) => Interlocked.Exchange(ref _value, newValue);
}

public class AtomicLong
{
    private long _value;

    public long Value
    {
        get => Volatile.Read(ref _value);
        set => Interlocked.Exchange(ref _value, value);
    }

    public AtomicLong(long initialValue = 0)
    {
        _value = initialValue;
    }

    public long Increment() => Interlocked.Increment(ref _value);
    
    public long Decrement() => Interlocked.Decrement(ref _value);
    
    public long Add(long value) => Interlocked.Add(ref _value, value);
    
    public bool CompareAndSet(long expected, long newValue) => Interlocked.CompareExchange(ref _value, newValue, expected) == expected;
    
    public long Exchange(long newValue) => Interlocked.Exchange(ref _value, newValue);
}

public class AtomicBool
{
    private int _value;

    public bool Value
    {
        get => Volatile.Read(ref _value) != 0;
        set => Interlocked.Exchange(ref _value, value ? 1 : 0);
    }

    public AtomicBool(bool initialValue = false)
    {
        _value = initialValue ? 1 : 0;
    }

    public bool CompareAndSet(bool expected, bool newValue)
    {
        var expectedInt = expected ? 1 : 0;
        var newInt = newValue ? 1 : 0;
        return Interlocked.CompareExchange(ref _value, newInt, expectedInt) == expectedInt;
    }

    public bool Exchange(bool newValue)
    {
        return Interlocked.Exchange(ref _value, newValue ? 1 : 0) != 0;
    }

    public bool GetAndSet(bool newValue)
    {
        return Interlocked.Exchange(ref _value, newValue ? 1 : 0) != 0;
    }
}