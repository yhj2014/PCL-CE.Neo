using System;
using System.IO;
using System.Linq;

namespace PCL_CE.Neo.Core.Utils.Validate;

public class FolderNameValidator : ValidatorBase<string?>
{
    private static readonly char[] InvalidChars = Path.GetInvalidPathChars();
    
    public override bool Validate(string? value, out string? errorMessage)
    {
        if (string.IsNullOrEmpty(value))
        {
            errorMessage = "Folder name cannot be null or empty";
            return false;
        }
        
        if (value.Any(c => InvalidChars.Contains(c)))
        {
            errorMessage = "Folder name contains invalid characters";
            return false;
        }
        
        if (value.Trim() != value)
        {
            errorMessage = "Folder name cannot start or end with whitespace";
            return false;
        }
        
        errorMessage = null;
        return true;
    }
}