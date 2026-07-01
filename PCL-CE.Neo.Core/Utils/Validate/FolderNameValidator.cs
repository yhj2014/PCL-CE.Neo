namespace PCL_CE.Neo.Core.Utils.Validate;

public class FolderNameValidator : IValidator<string>
{
    private static readonly char[] InvalidChars = System.IO.Path.GetInvalidPathChars();

    public bool Validate(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return false;

        if (value.IndexOfAny(InvalidChars) >= 0)
            return false;

        if (value.Any(c => char.IsControl(c)))
            return false;

        if (value.Trim() != value)
            return false;

        var reservedNames = new[] { "CON", "PRN", "AUX", "NUL", "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9", "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9" };
        if (reservedNames.Contains(value.ToUpperInvariant()))
            return false;

        return true;
    }

    public string GetErrorMessage(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return "文件夹名称不能为空。";

        if (value.IndexOfAny(InvalidChars) >= 0)
            return "文件夹名称包含无效字符。";

        if (value.Any(c => char.IsControl(c)))
            return "文件夹名称包含控制字符。";

        if (value.Trim() != value)
            return "文件夹名称不能以空白字符开头或结尾。";

        var reservedNames = new[] { "CON", "PRN", "AUX", "NUL", "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9", "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9" };
        if (reservedNames.Contains(value.ToUpperInvariant()))
            return $"'{value}' 是系统保留名称。";

        return string.Empty;
    }

    public static bool IsValidFolderName(string? name)
    {
        return new FolderNameValidator().Validate(name);
    }
}