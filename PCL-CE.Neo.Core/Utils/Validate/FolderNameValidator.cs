using System.IO;

namespace PCL_CE.Neo.Core.Utils.Validate;

public class FolderNameValidator
{
    private static readonly HashSet<string> ReservedNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "CON", "PRN", "AUX", "NUL",
        "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9",
        "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9"
    };

    public bool Validate(string? folderName)
    {
        if (string.IsNullOrWhiteSpace(folderName))
            return false;

        if (ReservedNames.Contains(folderName))
            return false;

        var invalidChars = Path.GetInvalidFileNameChars();
        return !folderName.Any(c => invalidChars.Contains(c));
    }
}