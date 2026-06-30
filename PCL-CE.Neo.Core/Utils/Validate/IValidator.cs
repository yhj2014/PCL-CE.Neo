using System;

namespace PCL_CE.Neo.Core.Utils.Validate;

public interface IValidator<in T>
{
    bool Validate(T value, out string? errorMessage);
}

public abstract class ValidatorBase<T> : IValidator<T>
{
    public abstract bool Validate(T value, out string? errorMessage);
    
    public bool Validate(T value)
    {
        return Validate(value, out _);
    }
}