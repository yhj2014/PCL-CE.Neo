using System.Collections.Generic;
using System.Linq;

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

    public ValidationResult Validate(string value)
    {
        var errors = new List<string>();

        if (value == null)
        {
            return ValidationResult.Success;
        }

        var length = value.Length;

        if (Min == Max && length != Max)
        {
            errors.Add($"长度必须为 {Max} 个字符！");
        }
        else
        {
            if (length < Min)
            {
                errors.Add($"长度至少为 {Min} 个字符！");
            }

            if (length > Max)
            {
                errors.Add($"长度最长为 {Max} 个字符！");
            }
        }

        return errors.Any() ? ValidationResult.Failure(errors) : ValidationResult.Success;
    }
}