using System;
using System.Collections.Generic;

namespace PCL.CE.Neo.Core.Utils;

public class Result<T>
{
    public bool Success { get; }
    public T? Value { get; }
    public string? ErrorMessage { get; }
    public Exception? Exception { get; }

    private Result(bool success, T? value, string? errorMessage, Exception? exception)
    {
        Success = success;
        Value = value;
        ErrorMessage = errorMessage;
        Exception = exception;
    }

    public static Result<T> Ok(T value) => new(true, value, null, null);
    
    public static Result<T> Fail(string errorMessage) => new(false, default, errorMessage, null);
    
    public static Result<T> Fail(Exception exception) => new(false, default, exception.Message, exception);
    
    public static Result<T> Fail(string errorMessage, Exception exception) => new(false, default, errorMessage, exception);

    public static implicit operator bool(Result<T> result) => result.Success;

    public T Unwrap()
    {
        if (!Success)
        {
            throw new InvalidOperationException(ErrorMessage ?? "Operation failed");
        }
        return Value!;
    }

    public T UnwrapOr(T defaultValue)
    {
        return Success ? Value! : defaultValue;
    }

    public T UnwrapOrElse(Func<T> fallback)
    {
        return Success ? Value! : fallback();
    }

    public Result<TResult> Map<TResult>(Func<T, TResult> mapper)
    {
        if (!Success)
        {
            return Result<TResult>.Fail(ErrorMessage ?? "Operation failed", Exception);
        }
        try
        {
            return Result<TResult>.Ok(mapper(Value!));
        }
        catch (Exception ex)
        {
            return Result<TResult>.Fail(ex);
        }
    }

    public async System.Threading.Tasks.Task<Result<TResult>> MapAsync<TResult>(Func<T, System.Threading.Tasks.Task<TResult>> mapper)
    {
        if (!Success)
        {
            return Result<TResult>.Fail(ErrorMessage ?? "Operation failed", Exception);
        }
        try
        {
            return Result<TResult>.Ok(await mapper(Value!));
        }
        catch (Exception ex)
        {
            return Result<TResult>.Fail(ex);
        }
    }

    public Result<T> OnSuccess(Action<T> action)
    {
        if (Success)
        {
            action(Value!);
        }
        return this;
    }

    public Result<T> OnFailure(Action<string?> action)
    {
        if (!Success)
        {
            action(ErrorMessage);
        }
        return this;
    }

    public Result<T> OnFailure(Action<string?, Exception?> action)
    {
        if (!Success)
        {
            action(ErrorMessage, Exception);
        }
        return this;
    }
}

public class Result
{
    public bool Success { get; }
    public string? ErrorMessage { get; }
    public Exception? Exception { get; }

    private Result(bool success, string? errorMessage, Exception? exception)
    {
        Success = success;
        ErrorMessage = errorMessage;
        Exception = exception;
    }

    public static Result Ok() => new(true, null, null);
    
    public static Result Fail(string errorMessage) => new(false, errorMessage, null);
    
    public static Result Fail(Exception exception) => new(false, exception.Message, exception);
    
    public static Result Fail(string errorMessage, Exception exception) => new(false, errorMessage, exception);

    public static Result<T> Ok<T>(T value) => Result<T>.Ok(value);
    
    public static Result<T> Fail<T>(string errorMessage) => Result<T>.Fail(errorMessage);
    
    public static Result<T> Fail<T>(Exception exception) => Result<T>.Fail(exception);

    public static implicit operator bool(Result result) => result.Success;

    public Result OnSuccess(Action action)
    {
        if (Success)
        {
            action();
        }
        return this;
    }

    public Result OnFailure(Action<string?> action)
    {
        if (!Success)
        {
            action(ErrorMessage);
        }
        return this;
    }

    public Result OnFailure(Action<string?, Exception?> action)
    {
        if (!Success)
        {
            action(ErrorMessage, Exception);
        }
        return this;
    }

    public Result<T> Then<T>(Func<T> func)
    {
        if (!Success)
        {
            return Result<T>.Fail(ErrorMessage ?? "Operation failed", Exception);
        }
        try
        {
            return Result<T>.Ok(func());
        }
        catch (Exception ex)
        {
            return Result<T>.Fail(ex);
        }
    }

    public async System.Threading.Tasks.Task<Result<T>> ThenAsync<T>(Func<System.Threading.Tasks.Task<T>> func)
    {
        if (!Success)
        {
            return Result<T>.Fail(ErrorMessage ?? "Operation failed", Exception);
        }
        try
        {
            return Result<T>.Ok(await func());
        }
        catch (Exception ex)
        {
            return Result<T>.Fail(ex);
        }
    }
}