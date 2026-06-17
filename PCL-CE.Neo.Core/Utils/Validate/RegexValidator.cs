using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace PCL_CE.Neo.Core.Utils.Validate;

public class RegexValidator : IValidator<string>
{
    public string Pattern { get; set; }
    public string ErrorMessage { get; set; }

    public RegexValidator(string pattern = "", string errorMessage = "正则检查失败！")
    {
        Pattern = pattern;
        ErrorMessage = errorMessage;
    }

    public RegexValidator() : this(string.Empty)
    {
    }

    public ValidationResult Validate(string value)
    {
        if (string.IsNullOrEmpty(value))
            return ValidationResult.Success();

        if (!Regex.IsMatch(value, Pattern))
            return ValidationResult.Failure(ErrorMessage);

        return ValidationResult.Success();
    }
}