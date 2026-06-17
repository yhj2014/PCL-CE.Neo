using PCL_CE.Neo.Core.Utils;
using System.Collections.Generic;

namespace PCL_CE.Neo.Core.Utils.Validate;

public class HttpAndUncValidator(bool allowNullOrEmpty = false) : IValidator<string>
{
    public bool AllowsNullOrEmpty { get; set; } = allowNullOrEmpty;

    public ValidationResult Validate(string value)
    {
        var errors = new List<string>();

        if (AllowsNullOrEmpty && string.IsNullOrEmpty(value))
        {
            return ValidationResult.Success();
        }

        if (string.IsNullOrEmpty(value))
        {
            errors.Add("输入内容不能为空！");
            return ValidationResult.Failure(errors);
        }

        if (!RegexPatterns.HttpUri.IsMatch(value) && !RegexPatterns.UncPath.IsMatch(value))
        {
            errors.Add("输入的网址无效！");
        }

        return errors.Count == 0 ? ValidationResult.Success() : ValidationResult.Failure(errors);
    }
}