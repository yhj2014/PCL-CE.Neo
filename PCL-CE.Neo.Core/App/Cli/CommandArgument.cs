using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace PCL-CE.Neo.Core.App.Cli;

public abstract class CommandArgument
{
    public required string Key { get; init; }

    public required string ValueText { get; init; }

    public abstract ArgumentValueKind ValueKind { get; }

    public abstract bool TryCastValue<T>([NotNullWhen(true)] out T? value);

    public T? CastValue<T>()
    {
        var result = TryCastValue(out T? value);
        return result ? value : throw new InvalidCastException("Value type mismatch or cannot cast");
    }
}

public abstract class CommandArgument<TValue> : CommandArgument
{
    protected abstract TValue ParseValueText();

    private bool _isValueParsed = false;

    public TValue Value
    {
        get
        {
            if (_isValueParsed) return field;
            _isValueParsed = true;
            return field = ParseValueText();
        }
        protected init
        {
            field = value;
            _isValueParsed = true;
        }
    } = default!;

    public override bool TryCastValue<T>([NotNullWhen(true)] out T value)
    {
        if (Value is T v)
        {
            value = v;
            return true;
        }
        value = default!;
        if (typeof(T) == typeof(string))
        {
            Unsafe.As<T, string>(ref value) = ValueText;
#pragma warning disable CS8762
            return true;
#pragma warning restore CS8762
        }
        return false;
    }
}