using System.Collections.Generic;

namespace PCL.CE.Neo.Core.Utils.Validate;

public class HttpAndUncValidator : IValidator<string>
{
    public bool AllowNullOrEmpty { get; }

    public HttpAndUncValidator(bool allowNullOrEmpty = false)
    {
        AllowNullOrEmpty = allowNullOrEmpty;
    }

    public ValidationResult Validate(string value)
    {
        if (AllowNullOrEmpty && string.IsNullOrEmpty(value))
        {
            return ValidationResult.Success;
        }

        if (!value.IsMatch(RegexPatterns.HttpAndUncUri))
        {
            return ValidationResult.Failure("输入的网址或 UNC 路径无效！");
        }
        return ValidationResult.Success;
    }
}