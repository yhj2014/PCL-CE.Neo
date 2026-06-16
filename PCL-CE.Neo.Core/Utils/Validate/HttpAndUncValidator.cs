using System.Text.RegularExpressions;

namespace PCL_CE.Neo.Core.Utils.Validate;

public class HttpAndUncValidator
{
    private static readonly Regex HttpRegex = new(@"^https?:\/\/[\w\-\._~:/?#\[\]@!$&'()*+,;=%]+$", RegexOptions.Compiled);
    private static readonly Regex UncRegex = new(@"^\\\\[\w\-\._~]+(\\[\w\-\._~]+)*$", RegexOptions.Compiled);

    public static ValidationResult Validate(string path, string fieldName)
    {
        if (!HttpRegex.IsMatch(path) && !UncRegex.IsMatch(path))
            return ValidationResult.Failure($"{fieldName} 不是有效的 HTTP/HTTPS 地址或 UNC 路径");
        return ValidationResult.Success();
    }
}