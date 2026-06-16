using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace PCL_CE.Neo.Core.Utils;

[DebuggerDisplay("{" + nameof(_DebuggerDisplay) + "}")]
public sealed class Result<TOk, TErr>
{
    private readonly TOk? _value;
    private readonly TErr? _error;
    private readonly bool _isSuccess;

    private Result(TOk value)
    {
        _value = value;
        _isSuccess = true;
    }

    private Result(TErr error)
    {
        _error = error;
        _isSuccess = false;
    }

    public static Result<TOk, TErr> Ok(TOk value) => new(value);
    public static Result<TOk, TErr> Err(TErr error) => new(error);

    public bool IsSuccess => _isSuccess;
    public bool IsFailure => !_isSuccess;

    public TOk Value => IsSuccess ? _value! : throw new InvalidOperationException("Cannot access Value of a failed Result.");
    public TErr Error => IsFailure ? _error! : throw new InvalidOperationException("Cannot access Error of a successful Result.");

    public TR Match<TR>(Func<TOk, TR> onSuccess, Func<TErr, TR> onError)
        => IsSuccess ? onSuccess(_value!) : onError(_error!);

    public void IfSuccess(Action<TOk> action)
    {
        if (IsSuccess) action(_value!);
    }

    public void IfFailure(Action<TErr> action)
    {
        if (IsFailure) action(_error!);
    }

    public Result<T2, TErr> Map<T2>(Func<TOk, T2> mapper)
        => IsSuccess ? Result<T2, TErr>.Ok(mapper(_value!)) : Result<T2, TErr>.Err(_error!);

    public Result<TOk, TErr2> MapError<TErr2>(Func<TErr, TErr2> mapper)
        => IsSuccess ? Result<TOk, TErr2>.Ok(_value!) : Result<TOk, TErr2>.Err(mapper(_error!));

    public Result<T2, TErr> Bind<T2>(Func<TOk, Result<T2, TErr>> mapper)
        => IsSuccess ? mapper(_value!) : Result<T2, TErr>.Err(_error!);

    public Result<TOk, TErr2> BindError<TErr2>(Func<TErr, Result<TOk, TErr2>> mapper)
        => IsFailure ? mapper(_error!) : Result<TOk, TErr2>.Ok(_value!);

    public TOk OrElse(TOk defaultValue) => IsSuccess ? _value! : defaultValue;
    public TOk OrElse(Func<TOk> defaultValueFactory) => IsSuccess ? _value! : defaultValueFactory();

    public TOk Expect(string message) => IsSuccess ? _value! : throw new InvalidOperationException(message);

    public TOk UnwrapOrThrow<TException>(Func<TErr, TException> exceptionFactory) where TException : Exception
        => IsSuccess ? _value! : throw exceptionFactory(_error!);

    private string _DebuggerDisplay => IsSuccess ? $"Ok({_value})" : $"Err({_error})";
    public override string ToString() => _DebuggerDisplay;

    public override bool Equals(object? obj)
    {
        if (obj is not Result<TOk, TErr> other) return false;
        if (IsSuccess != other.IsSuccess) return false;
        return IsSuccess
            ? EqualityComparer<TOk>.Default.Equals(_value, other._value)
            : EqualityComparer<TErr>.Default.Equals(_error, other._error);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(IsSuccess, _value, _error);
    }
}