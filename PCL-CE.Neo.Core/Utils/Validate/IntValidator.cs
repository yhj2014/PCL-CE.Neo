namespace PCL_CE.Neo.Core.Utils.Validate;

public class IntValidator
{
    public static ValidationResult Validate(int value, int minValue, int maxValue, string fieldName)
    {
        if (value < minValue)
            return ValidationResult.Failure($"{fieldName} 不能小于 {minValue}");
        if (value > maxValue)
            return ValidationResult.Failure($"{fieldName} 不能大于 {maxValue}");
        return ValidationResult.Success();
    }
}