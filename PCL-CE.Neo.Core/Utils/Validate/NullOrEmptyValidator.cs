using System.Collections.Generic;

namespace PCL.CE.Neo.Core.Utils.Validate;

public class NullOrEmptyValidator : IValidator<string>
{
    public ValidationResult Validate(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return ValidationResult.Failure("输入内容不能为空！");
        }
        return ValidationResult.Success;
    }
}