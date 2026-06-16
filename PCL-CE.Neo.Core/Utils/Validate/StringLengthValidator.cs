namespace PCL_CE.Neo.Core.Utils.Validate;

public class StringLengthValidator
{
    public static ValidationResult Validate(string value, int minLength, int maxLength, string fieldName)
    {
        if (value.Length < minLength)
            return ValidationResult.Failure($"{fieldName} 长度不能小于 {minLength} 个字符");
        if (value.Length > maxLength)
            return ValidationResult.Failure($"{fieldName} 长度不能大于 {maxLength} 个字符");
        return ValidationResult.Success();
    }
}