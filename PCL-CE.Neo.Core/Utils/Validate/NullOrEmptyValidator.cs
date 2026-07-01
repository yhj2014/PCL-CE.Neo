namespace PCL_CE.Neo.Core.Utils.Validate;

public class NullOrEmptyValidator : IValidator<string>
{
    public bool Validate(string? value)
    {
        return !string.IsNullOrEmpty(value);
    }

    public string GetErrorMessage(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return "值不能为空。";

        return string.Empty;
    }

    public static bool IsNotNullOrEmpty(string? value)
    {
        return !string.IsNullOrEmpty(value);
    }
}