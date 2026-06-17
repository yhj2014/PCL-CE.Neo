using System.Collections.Generic;

namespace PCL_CE.Neo.Core.Utils.Validate;

public class StringLengthValidator : IValidator<string>
{
    public int Min { get; set; }
    public int Max { get; set; }

    public StringLengthValidator(int min = 0, int max = int.MaxValue)
    {
        Min = min;
        Max = max;
    }

    public StringLengthValidator() : this(0)
    {
    }

    public ValidationResult Validate(string value)
    {
        var errors = new List<string>();

        if (string.IsNullOrEmpty(value))
            return ValidationResult.Success();

        if (Max == Min && value.Length != Max)
            errors.Add($"长度必须为 {Max} 个字符！");

        if (value.Length < Min)
            errors.Add($"长度至少为 {Min} 个字符！");

        if (value.Length > Max)
            errors.Add($"长度最长为 {Max} 个字符！");

        return errors.Count == 0 ? ValidationResult.Success() : ValidationResult.Failure(errors);
    }
}