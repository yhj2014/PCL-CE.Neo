using System.Collections.Generic;

namespace PCL_CE.Neo.Core.Utils.Validate;

public class IntValidator : IValidator<string>
{
    public int Max { get; set; }
    public int Min { get; set; }

    public IntValidator(int max = int.MaxValue, int min = int.MinValue)
    {
        Max = max;
        Min = min;
    }

    public IntValidator() : this(int.MaxValue)
    {
    }

    public ValidationResult Validate(string value)
    {
        var errors = new List<string>();

        if (string.IsNullOrEmpty(value))
            return ValidationResult.Success();

        if (value.Length >= 9)
            errors.Add("请输入一个大小合理的数字！");

        if (!int.TryParse(value, out var parsedValue))
            errors.Add("请输入一个整数！");
        else
        {
            if (parsedValue > Max)
                errors.Add($"不可超过 {Max}！");

            if (parsedValue < Min)
                errors.Add($"不可低于 {Min}！");
        }

        return errors.Count == 0 ? ValidationResult.Success() : ValidationResult.Failure(errors);
    }
}