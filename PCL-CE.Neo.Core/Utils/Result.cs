using System;
using System.Diagnostics.CodeAnalysis;

namespace PCL_CE.Neo.Core.Utils;

public readonly struct Result<T> : IEquatable<Result<T>>
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public T? Value { get; }
    public string? Error { get; }
    public Exception? Exception { get; }

    private Result(T value)
    {
        IsSuccess = true;
        Value = value;
        Error = null;
        Exception = null;
    }

    private Result(string error, Exception? exception = null)
    {
        IsSuccess = false;
        Value = default;
        Error = error;
        Exception = exception;
    }

    public static Result<T> Success(T value) => new(value);

    public static Result<T> Failure(string error) => new(error);

    public static Result<T> Failure(string error, Exception exception) => new(error, exception);

    public static Result<T> FromException(Exception exception) => new(exception.Message, exception);

    public static implicit operator Result<T>(T value) => Success(value);

    public static implicit operator Result<T>((bool success, T value) tuple) => tuple.success ? Success(tuple.value) : Failure("Unknown error");

    public R Match<R>(Func<T, R> success, Func<string, Exception?, R> failure)
    {
        return IsSuccess ? success(Value!) : failure(Error!, Exception);
    }

    public bool TryGetValue([NotNullWhen(true)] out T? value)
    {
        value = Value;
        return IsSuccess;
    }

    public bool Equals(Result<T> other)
    {
        return IsSuccess == other.IsSuccess &&
               EqualityComparer<T?>.Default.Equals(Value, other.Value) &&
               Error == other.Error;
    }

    public override bool Equals(object? obj)
    {
        return obj is Result<T> other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(IsSuccess, Value, Error);
    }

    public override string ToString()
    {
        return IsSuccess ? $"Success({Value})" : $"Failure({Error})";
    }
}

public readonly struct Result : IEquatable<Result>
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string? Error { get; }
    public Exception? Exception { get; }

    private Result(bool isSuccess, string? error = null, Exception? exception = null)
    {
        IsSuccess = isSuccess;
        Error = error;
        Exception = exception;
    }

    public static Result Success() => new(true);

    public static Result Failure(string error) => new(false, error);

    public static Result Failure(string error, Exception exception) => new(false, error, exception);

    public static Result FromException(Exception exception) => new(false, exception.Message, exception);

    public static implicit operator Result(bool success) => success ? Success() : Failure("Operation failed");

    public R Match<R>(Func<R> success, Func<string, Exception?, R> failure)
    {
        return IsSuccess ? success() : failure(Error!, Exception);
    }

    public bool Equals(Result other)
    {
        return IsSuccess == other.IsSuccess && Error == other.Error;
    }

    public override bool Equals(object? obj)
    {
        return obj is Result other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(IsSuccess, Error);
    }

    public override string ToString()
    {
        return IsSuccess ? "Success" : $"Failure({Error})";
    }
}