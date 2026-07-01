namespace PCL_CE.Neo.Core.Utils.Validate;

public class HttpAndUncValidator : IValidator<string>
{
    public bool Validate(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return false;

        value = value.Trim();

        return HttpValidator.IsValidUrl(value) || IsValidUncPath(value);
    }

    public string GetErrorMessage(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return "URL 不能为空。";

        if (!Validate(value))
            return "无效的 HTTP/HTTPS URL 或 UNC 路径。";

        return string.Empty;
    }

    public static bool IsValidUncPath(string? path)
    {
        if (string.IsNullOrEmpty(path))
            return false;

        return path.StartsWith(@"\\") && path.Length > 2;
    }

    public static bool IsValid(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return false;

        value = value.Trim();

        return HttpValidator.IsValidUrl(value) || IsValidUncPath(value);
    }
}