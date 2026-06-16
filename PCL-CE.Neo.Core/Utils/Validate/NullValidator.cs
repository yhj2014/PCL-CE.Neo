namespace PCL_CE.Neo.Core.Utils.Validate;

public class NullValidator
{
    public static ValidationResult Validate(string? value, string fieldName)
    {
        if (value == null)
            return ValidationResult.Failure($"{fieldName} 不能为空引用");
        return ValidationResult.Success();
    }
}