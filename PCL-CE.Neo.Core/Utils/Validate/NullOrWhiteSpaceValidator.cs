namespace PCL_CE.Neo.Core.Utils.Validate;

public class NullOrWhiteSpaceValidator
{
    public static ValidationResult Validate(string? value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
            return ValidationResult.Failure($"{fieldName} 不能为空或空白字符");
        return ValidationResult.Success();
    }
}