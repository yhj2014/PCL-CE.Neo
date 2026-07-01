namespace PCL_CE.Neo.Core.Utils;

public class Result<T>
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public T? Value { get; }
    public string? ErrorMessage { get; }
    public Exception? Exception { get; }

    private Result(bool isSuccess, T? value, string? errorMessage, Exception? exception)
    {
        IsSuccess = isSuccess;
        Value = value;
        ErrorMessage = errorMessage;
        Exception = exception;
    }

    public static Result<T> Success(T value)
    {
        return new Result<T>(true, value, null, null);
    }

    public static Result<T> Failure(string errorMessage)
    {
        return new Result<T>(false, default, errorMessage, null);
    }

    public static Result<T> Failure(Exception exception)
    {
        return new Result<T>(false, default, exception.Message, exception);
    }

    public static Result<T> Failure(string errorMessage, Exception exception)
    {
        return new Result<T>(false, default, errorMessage, exception);
    }

    public T GetValueOrThrow()
    {
        if (IsFailure)
            throw new InvalidOperationException(ErrorMessage ?? "操作失败");
        return Value!;
    }

    public T GetValueOrDefault(T defaultValue)
    {
        return IsSuccess ? Value! : defaultValue;
    }

    public TOut Match<TOut>(Func<T, TOut> onSuccess, Func<string?, TOut> onFailure)
    {
        return IsSuccess ? onSuccess(Value!) : onFailure(ErrorMessage);
    }

    public void Match(Action<T> onSuccess, Action<string?> onFailure)
    {
        if (IsSuccess)
            onSuccess(Value!);
        else
            onFailure(ErrorMessage);
    }

    public Result<TOut> Bind<TOut>(Func<T, Result<TOut>> func)
    {
        return IsSuccess ? func(Value!) : Result<TOut>.Failure(ErrorMessage!);
    }

    public Result<TOut> Map<TOut>(Func<T, TOut> func)
    {
        return IsSuccess ? Result<TOut>.Success(func(Value!)) : Result<TOut>.Failure(ErrorMessage!);
    }

    public Result<T> OnSuccess(Action<T> action)
    {
        if (IsSuccess)
            action(Value!);
        return this;
    }

    public Result<T> OnFailure(Action<string?> action)
    {
        if (IsFailure)
            action(ErrorMessage);
        return this;
    }
}

public class Result
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

    public static Result Success()
    {
        return new Result(true, null, null);
    }

    public static Result Failure(string errorMessage)
    {
        return new Result(false, errorMessage, null);
    }

    public static Result Failure(Exception exception)
    {
        return new Result(false, exception.Message, exception);
    }

    public static Result Failure(string errorMessage, Exception exception)
    {
        return new Result(false, errorMessage, exception);
    }

    public static Result<T> Success<T>(T value)
    {
        return Result<T>.Success(value);
    }

    public static Result<T> Failure<T>(string errorMessage)
    {
        return Result<T>.Failure(errorMessage);
    }

    public static Result<T> Failure<T>(Exception exception)
    {
        return Result<T>.Failure(exception);
    }

    public void ThrowIfFailure()
    {
        if (IsFailure)
            throw new InvalidOperationException(ErrorMessage ?? "操作失败");
    }

    public TOut Match<TOut>(Func<TOut> onSuccess, Func<string?, TOut> onFailure)
    {
        return IsSuccess ? onSuccess() : onFailure(ErrorMessage);
    }

    public void Match(Action onSuccess, Action<string?> onFailure)
    {
        if (IsSuccess)
            onSuccess();
        else
            onFailure(ErrorMessage);
    }

    public Result Bind(Func<Result> func)
    {
        return IsSuccess ? func() : Failure(ErrorMessage!);
    }

    public Result<TOut> Bind<TOut>(Func<Result<TOut>> func)
    {
        return IsSuccess ? func() : Result<TOut>.Failure(ErrorMessage!);
    }

    public Result OnSuccess(Action action)
    {
        if (IsSuccess)
            action();
        return this;
    }

    public Result OnFailure(Action<string?> action)
    {
        if (IsFailure)
            action(ErrorMessage);
        return this;
    }
}