using System.Collections.Generic;

namespace PCL_CE.Neo.Core.Utils.Validate;

public interface IValidator<in T>
{
    ValidationResult Validate(T value);
}

public class ValidationResult
{
    public bool IsValid { get; }
    public List<string> Errors { get; }

    private ValidationResult(bool isValid, List<string> errors)
    {
        IsValid = isValid;
        Errors = errors;
    }

    public static ValidationResult Success() => new(true, []);
    public static ValidationResult Failure(params string[] errors) => new(false, errors.ToList());
    public static ValidationResult Failure(List<string> errors) => new(false, errors);
}