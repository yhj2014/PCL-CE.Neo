using System.Collections.Generic;

namespace PCL.CE.Neo.Core.Utils.Validate;

public class NullOrWhiteSpaceValidator : IValidator<string>
{
    public ValidationResult Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return ValidationResult.Failure("输入内容不能为空或仅包含空白字符！");
        }
        return ValidationResult.Success;
    }
}