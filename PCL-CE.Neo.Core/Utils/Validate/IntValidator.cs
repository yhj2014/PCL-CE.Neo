using System.Collections.Generic;

namespace PCL.CE.Neo.Core.Utils.Validate;

public class IntValidator : IValidator<string>
{
    public int? MinValue { get; set; }
    public int? MaxValue { get; set; }

    public ValidationResult Validate(string value)
    {
        if (!int.TryParse(value, out var intValue))
        {
            return ValidationResult.Failure("输入必须是整数！");
        }

        if (MinValue.HasValue && intValue < MinValue.Value)
        {
            return ValidationResult.Failure($"输入必须大于等于 {MinValue.Value}！");
        }

        if (MaxValue.HasValue && intValue > MaxValue.Value)
        {
            return ValidationResult.Failure($"输入必须小于等于 {MaxValue.Value}！");
        }

        return ValidationResult.Success;
    }
}