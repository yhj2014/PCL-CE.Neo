using System.Text.RegularExpressions;

namespace PCL_CE.Neo.Core.Utils.Validate;

public class RegexValidator : IValidator<string>
{
    private readonly Regex _pattern;

    public RegexValidator(string pattern)
    {
        _pattern = new Regex(pattern);
    }

    public RegexValidator(string pattern, RegexOptions options)
    {
        _pattern = new Regex(pattern, options);
    }

    public bool Validate(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return false;

        return _pattern.IsMatch(value);
    }

    public string GetErrorMessage(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return "值不能为空";

        if (_pattern.IsMatch(value))
            return string.Empty;

        return $"值不符合正则表达式模式: {_pattern}";
    }
}