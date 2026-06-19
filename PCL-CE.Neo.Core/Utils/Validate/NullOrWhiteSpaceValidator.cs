namespace PCL_CE.Neo.Core.Utils.Validate;

public class NullOrWhiteSpaceValidator
{
    public bool Validate(string? value)
    {
        return !string.IsNullOrWhiteSpace(value);
    }

    public string? ValidateAndGetError(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "输入内容不能为空！";
        return null;
    }
}