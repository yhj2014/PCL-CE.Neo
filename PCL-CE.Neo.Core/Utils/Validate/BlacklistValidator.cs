using System.Linq;

namespace PCL_CE.Neo.Core.Utils.Validate;

public class BlacklistValidator
{
    private static readonly string[] BlacklistedKeywords =
    {
        "..", "\\\\", "/", ":", "*", "?", "\"", "<", ">", "|"
    };

    public static ValidationResult Validate(string value, string fieldName)
    {
        if (BlacklistedKeywords.Any(keyword => value.Contains(keyword)))
            return ValidationResult.Failure($"{fieldName} 包含非法字符或关键字");
        return ValidationResult.Success();
    }

    public static ValidationResult Validate(string value, string[] customBlacklist, string fieldName)
    {
        if (customBlacklist.Any(keyword => value.Contains(keyword)))
            return ValidationResult.Failure($"{fieldName} 包含非法关键字");
        return ValidationResult.Success();
    }
}