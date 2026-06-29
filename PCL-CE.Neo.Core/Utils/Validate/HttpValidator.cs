using System.Collections.Generic;

namespace PCL.CE.Neo.Core.Utils.Validate;

public class HttpValidator : IValidator<string>
{
    public bool AllowNullOrEmpty { get; }

    public HttpValidator(bool allowNullOrEmpty = false)
    {
        AllowNullOrEmpty = allowNullOrEmpty;
    }

    public ValidationResult Validate(string value)
    {
        if (AllowNullOrEmpty && string.IsNullOrEmpty(value))
        {
            return ValidationResult.Success;
        }

        if (!value.IsMatch(RegexPatterns.HttpUri))
        {
            return ValidationResult.Failure("输入的网址无效！");
        }
        return ValidationResult.Success;
    }
}