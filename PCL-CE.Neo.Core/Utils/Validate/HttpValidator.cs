using System.Text.RegularExpressions;

namespace PCL_CE.Neo.Core.Utils.Validate;

public class HttpValidator
{
    private readonly bool _allowNullOrEmpty;

    public bool AllowsNullOrEmpty { get; set; }

    public HttpValidator(bool allowNullOrEmpty)
    {
        _allowNullOrEmpty = allowNullOrEmpty;
        AllowsNullOrEmpty = allowNullOrEmpty;
    }

    public HttpValidator() : this(false)
    {
    }

    public bool Validate(string value)
    {
        if (AllowsNullOrEmpty && string.IsNullOrEmpty(value))
        {
            return true;
        }

        return Regex.IsMatch(value, RegexPatterns.HttpUri);
    }

    public string? ValidateAndGetError(string value)
    {
        if (AllowsNullOrEmpty && string.IsNullOrEmpty(value))
        {
            return null;
        }

        if (!Regex.IsMatch(value, RegexPatterns.HttpUri))
        {
            return "输入的网址无效！";
        }

        return null;
    }
}