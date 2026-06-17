using System.Collections.Generic;

namespace PCL_CE.Neo.Core.Utils.Validate;

public class BlacklistValidator : IValidator<string>
{
    public List<string> Blacklist { get; set; }

    public BlacklistValidator(List<string> contains)
    {
        Blacklist = contains;
    }

    public BlacklistValidator() : this([])
    {
    }

    public ValidationResult Validate(string value)
    {
        var errors = new List<string>();

        if (string.IsNullOrEmpty(value))
            return ValidationResult.Success();

        foreach (var item in Blacklist)
        {
            if (value.Contains(item))
            {
                errors.Add($"输入内容不能包含 {item}！");
            }
        }

        return errors.Count == 0 ? ValidationResult.Success() : ValidationResult.Failure(errors);
    }
}