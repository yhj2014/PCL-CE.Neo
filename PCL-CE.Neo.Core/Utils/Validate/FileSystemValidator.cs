namespace PCL_CE.Neo.Core.Utils.Validate;

public class FileSystemValidator : IValidator<string>
{
    private readonly bool _allowExists;
    private readonly bool _allowNotExists;

    public FileSystemValidator(bool allowExists = true, bool allowNotExists = true)
    {
        _allowExists = allowExists;
        _allowNotExists = allowNotExists;
    }

    public bool Validate(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return false;

        var exists = System.IO.File.Exists(value) || System.IO.Directory.Exists(value);

        return (exists && _allowExists) || (!exists && _allowNotExists);
    }

    public string GetErrorMessage(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return "路径不能为空。";

        var exists = System.IO.File.Exists(value) || System.IO.Directory.Exists(value);

        if (exists && !_allowExists)
            return $"路径 '{value}' 已存在。";

        if (!exists && !_allowNotExists)
            return $"路径 '{value}' 不存在。";

        return string.Empty;
    }

    public static bool Exists(string? path)
    {
        if (string.IsNullOrEmpty(path))
            return false;

        return System.IO.File.Exists(path) || System.IO.Directory.Exists(path);
    }

    public static bool IsFile(string? path)
    {
        if (string.IsNullOrEmpty(path))
            return false;

        return System.IO.File.Exists(path);
    }

    public static bool IsDirectory(string? path)
    {
        if (string.IsNullOrEmpty(path))
            return false;

        return System.IO.Directory.Exists(path);
    }
}