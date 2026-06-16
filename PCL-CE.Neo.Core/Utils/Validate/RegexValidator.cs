using System.Text.RegularExpressions;

namespace PCL_CE.Neo.Core.Utils.Validate;

public class RegexValidator
{
    public static ValidationResult Validate(string value, string pattern, string fieldName)
    {
        if (!Regex.IsMatch(value, pattern))
            return ValidationResult.Failure($"{fieldName} 格式不正确");
        return ValidationResult.Success();
    }

    public static ValidationResult Validate(string value, Regex regex, string fieldName)
    {
        if (!regex.IsMatch(value))
            return ValidationResult.Failure($"{fieldName} 格式不正确");
        return ValidationResult.Success();
    }
}