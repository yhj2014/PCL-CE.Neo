using System.Collections.Generic;

namespace PCL.CE.Neo.Core.Utils.Validate;

public interface IValidator<T>
{
    ValidationResult Validate(T value);
}

public class ValidationResult
{
    public bool IsValid { get; }
    public List<string> Errors { get; }

    public ValidationResult(bool isValid, List<string> errors)
    {
        IsValid = isValid;
        Errors = errors;
    }

    public static ValidationResult Success { get; } = new(true, []);
    
    public static ValidationResult Failure(List<string> errors) => new(false, errors);
    
    public static ValidationResult Failure(string error) => new(false, [error]);
}