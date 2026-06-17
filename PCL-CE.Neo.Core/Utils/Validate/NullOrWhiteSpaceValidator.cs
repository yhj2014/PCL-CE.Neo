using System.Collections.Generic;

namespace PCL_CE.Neo.Core.Utils.Validate;

public class NullOrWhiteSpaceValidator : IValidator<string>
{
    public ValidationResult Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return ValidationResult.Failure("输入内容不能为空！");
        }

        return ValidationResult.Success();
    }
}