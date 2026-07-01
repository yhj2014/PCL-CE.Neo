namespace PCL_CE.Neo.Core.Utils.Validate;

public class FolderPathValidator
{
    private static readonly char[] InvalidPathChars = Path.GetInvalidPathChars();

    public bool AllowNull { get; }

    public FolderPathValidator(bool allowNull = false)
    {
        AllowNull = allowNull;
    }

    public bool Validate(string? path)
    {
        if (path is null)
            return AllowNull;

        if (path.Length == 0)
            return false;

        if (path.Trim().Length == 0)
            return false;

        return !path.Any(c => InvalidPathChars.Contains(c));
    }

    public bool Validate(string? path, out string? errorMessage)
    {
        errorMessage = null;

        if (path is null)
        {
            if (!AllowNull)
                errorMessage = "Path cannot be null.";
            return AllowNull;
        }

        if (path.Length == 0)
        {
            errorMessage = "Path cannot be empty.";
            return false;
        }

        if (path.Trim().Length == 0)
        {
            errorMessage = "Path cannot contain only whitespace.";
            return false;
        }

        var invalidChars = path.Where(c => InvalidPathChars.Contains(c)).ToList();
        if (invalidChars.Count > 0)
        {
            errorMessage = $"Path contains invalid characters: {string.Join(", ", invalidChars.Distinct())}";
            return false;
        }

        return true;
    }

    public bool Exists(string? path)
    {
        if (!Validate(path))
            return false;

        return Directory.Exists(path);
    }

    public void ValidateAndThrow(string? path, string? paramName = null)
    {
        if (!Validate(path, out var errorMessage))
            throw new ArgumentException(errorMessage, paramName);
    }

    public void ValidateExistsAndThrow(string? path, string? paramName = null)
    {
        ValidateAndThrow(path, paramName);

        if (!Directory.Exists(path))
            throw new DirectoryNotFoundException($"Directory not found: {path}");
    }
}