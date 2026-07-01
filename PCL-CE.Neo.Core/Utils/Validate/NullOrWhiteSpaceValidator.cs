namespace PCL_CE.Neo.Core.Utils.Validate;

public class NullOrWhiteSpaceValidator : IValidator<string>
{
    public bool Validate(string? value)
    {
        return !string.IsNullOrWhiteSpace(value);
    }

    public string GetErrorMessage(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "值不能为空或仅包含空白字符。";

        return string.Empty;
    }

    public static bool IsNotNullOrWhiteSpace(string? value)
    {
        return !string.IsNullOrWhiteSpace(value);
    }
}