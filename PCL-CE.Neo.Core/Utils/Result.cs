using System;
using System.Diagnostics;

namespace PCL_CE.Neo.Core.Utils;

public readonly struct Result<T>
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public T? Value { get; }
    public string? ErrorMessage { get; }
    public Exception? Exception { get; }

    private Result(T value)
    {
        IsSuccess = true;
        Value = value;
        ErrorMessage = null;
        Exception = null;
    }

    private Result(string errorMessage, Exception? exception = null)
    {
        IsSuccess = false;
        Value = default;
        ErrorMessage = errorMessage;
        Exception = exception;
    }

    public static Result<T> Success(T value) => new(value);

    public static Result<T> Failure(string errorMessage) => new(errorMessage);

    public static Result<T> Failure(Exception exception) => new(exception.Message, exception);

    public static Result<T> Failure(string errorMessage, Exception exception) => new(errorMessage, exception);

    public static implicit operator Result<T>(T value) => Success(value);

    public static implicit operator Result<T>(string errorMessage) => Failure(errorMessage);

    public static implicit operator Result<T>(Exception exception) => Failure(exception);

    public T GetValueOrDefault(T defaultValue) => IsSuccess ? Value! : defaultValue;

    public T GetValueOrDefault(Func<T> defaultValueFactory) => IsSuccess ? Value! : defaultValueFactory();

    public void Deconstruct(out bool isSuccess, out T? value, out string? errorMessage)
    {
        isSuccess = IsSuccess;
        value = Value;
        errorMessage = ErrorMessage;
    }

    public override string ToString()
    {
        return IsSuccess ? $"Success: {Value}" : $"Failure: {ErrorMessage}";
    }
}

public readonly struct Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string? ErrorMessage { get; }
    public Exception? Exception { get; }

    private Result(bool isSuccess, string? errorMessage, Exception? exception)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
        Exception = exception;
    }

    public static Result Success() => new(true, null, null);

    public static Result Failure(string errorMessage) => new(false, errorMessage, null);

    public static Result Failure(Exception exception) => new(false, exception.Message, exception);

    public static Result Failure(string errorMessage, Exception exception) => new(false, errorMessage, exception);

    public static implicit operator Result(bool isSuccess) => isSuccess ? Success() : Failure("Operation failed");

    public static implicit operator Result(string errorMessage) => Failure(errorMessage);

    public static implicit operator Result(Exception exception) => Failure(exception);

    public void Deconstruct(out bool isSuccess, out string? errorMessage)
    {
        isSuccess = IsSuccess;
        errorMessage = ErrorMessage;
    }

    public override string ToString()
    {
        return IsSuccess ? "Success" : $"Failure: {ErrorMessage}";
    }
}