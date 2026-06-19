using System.Text.RegularExpressions;

namespace PCL_CE.Neo.Core.Utils.Validate;

public class HttpAndUncValidator
{
    private readonly bool _allowNullOrEmpty;

    public bool AllowsNullOrEmpty { get; set; }

    public HttpAndUncValidator(bool allowNullOrEmpty)
    {
        _allowNullOrEmpty = allowNullOrEmpty;
        AllowsNullOrEmpty = allowNullOrEmpty;
    }

    public HttpAndUncValidator() : this(false)
    {
    }

    public bool Validate(string value)
    {
        if (AllowsNullOrEmpty && string.IsNullOrEmpty(value))
        {
            return true;
        }

        return Regex.IsMatch(value, RegexPatterns.HttpUri) || Regex.IsMatch(value, RegexPatterns.UncPath);
    }

    public string? ValidateAndGetError(string value)
    {
        if (AllowsNullOrEmpty && string.IsNullOrEmpty(value))
        {
            return null;
        }

        if (!Regex.IsMatch(value, RegexPatterns.HttpUri) && !Regex.IsMatch(value, RegexPatterns.UncPath))
        {
            return "输入的网址或路径无效！";
        }

        return null;
    }
}