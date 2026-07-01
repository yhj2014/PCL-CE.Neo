namespace PCL_CE.Neo.Core.Utils.Validate;

public class FileNameValidator
{
    private static readonly char[] InvalidFileNameChars = Path.GetInvalidFileNameChars();

    public bool AllowNull { get; }

    public FileNameValidator(bool allowNull = false)
    {
        AllowNull = allowNull;
    }

    public bool Validate(string? fileName)
    {
        if (fileName is null)
            return AllowNull;

        if (fileName.Length == 0)
            return false;

        if (fileName.Trim().Length == 0)
            return false;

        return !fileName.Any(c => InvalidFileNameChars.Contains(c));
    }

    public bool Validate(string? fileName, out string? errorMessage)
    {
        errorMessage = null;

        if (fileName is null)
        {
            if (!AllowNull)
                errorMessage = "File name cannot be null.";
            return AllowNull;
        }

        if (fileName.Length == 0)
        {
            errorMessage = "File name cannot be empty.";
            return false;
        }

        if (fileName.Trim().Length == 0)
        {
            errorMessage = "File name cannot contain only whitespace.";
            return false;
        }

        var invalidChars = fileName.Where(c => InvalidFileNameChars.Contains(c)).ToList();
        if (invalidChars.Count > 0)
        {
            errorMessage = $"File name contains invalid characters: {string.Join(", ", invalidChars.Distinct())}";
            return false;
        }

        return true;
    }

    public void ValidateAndThrow(string? fileName, string? paramName = null)
    {
        if (!Validate(fileName, out var errorMessage))
            throw new ArgumentException(errorMessage, paramName);
    }
}