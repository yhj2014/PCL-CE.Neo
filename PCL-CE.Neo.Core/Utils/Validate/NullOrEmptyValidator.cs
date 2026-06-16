namespace PCL_CE.Neo.Core.Utils.Validate;

public class NullOrEmptyValidator
{
    public static ValidationResult Validate(string? value, string fieldName)
    {
        if (string.IsNullOrEmpty(value))
            return ValidationResult.Failure($"{fieldName} 不能为空");
        return ValidationResult.Success();
    }
}