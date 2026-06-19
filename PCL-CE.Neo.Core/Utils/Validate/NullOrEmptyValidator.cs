namespace PCL_CE.Neo.Core.Utils.Validate;

public class NullOrEmptyValidator
{
    public bool Validate(string? value)
    {
        return !string.IsNullOrEmpty(value);
    }

    public string? ValidateAndGetError(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return "输入内容不能为空！";
        return null;
    }
}