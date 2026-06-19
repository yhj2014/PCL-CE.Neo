using System.Runtime.CompilerServices;

namespace PCL_CE.Neo.Core.Utils;

public class AtomicVariable<T> where T : struct
{
    private T _value;

    public AtomicVariable(T initialValue = default)
    {
        _value = initialValue;
    }

    public T Value
    {
        get => Volatile.Read(ref _value);
        set => Volatile.Write(ref _value, value);
    }

    public T Exchange(T newValue)
    {
        return Interlocked.Exchange(ref _value, newValue);
    }

    public bool CompareExchange(T newValue, T comparand)
    {
        var original = Interlocked.CompareExchange(ref _value, newValue, comparand);
        return original.Equals(comparand);
    }
}

public static class Volatile
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Read<T>(ref T location) where T : struct
    {
        return Unsafe.ReadUnaligned<T>(ref Unsafe.As<T, byte>(ref location));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write<T>(ref T location, T value) where T : struct
    {
        Unsafe.WriteUnaligned(ref Unsafe.As<T, byte>(ref location), value);
    }
}