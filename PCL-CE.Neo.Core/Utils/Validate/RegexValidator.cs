using System.Text.RegularExpressions;

namespace PCL_CE.Neo.Core.Utils.Validate;

public class RegexValidator
{
    private readonly Regex _regex;

    public RegexValidator(string pattern)
    {
        _regex = new Regex(pattern);
    }

    public RegexValidator(string pattern, RegexOptions options)
    {
        _regex = new Regex(pattern, options);
    }

    public bool Validate(string input)
    {
        if (string.IsNullOrEmpty(input))
            return false;

        return _regex.IsMatch(input);
    }
}