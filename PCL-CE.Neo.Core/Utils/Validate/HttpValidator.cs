using System.Text.RegularExpressions;

namespace PCL_CE.Neo.Core.Utils.Validate;

public class HttpValidator
{
    private static readonly Regex HttpRegex = new(@"^https?:\/\/[\w\-\._~:/?#\[\]@!$&'()*+,;=%]+$", RegexOptions.Compiled);

    public static ValidationResult Validate(string url, string fieldName)
    {
        if (!HttpRegex.IsMatch(url))
            return ValidationResult.Failure($"{fieldName} 不是有效的 HTTP/HTTPS 地址");
        return ValidationResult.Success();
    }
}