using System;
using System.IO;
using System.Linq;

namespace PCL_CE.Neo.Core.Utils.Validate;

public class FileNameValidator : ValidatorBase<string?>
{
    private static readonly char[] InvalidChars = Path.GetInvalidFileNameChars();
    
    public override bool Validate(string? value, out string? errorMessage)
    {
        if (string.IsNullOrEmpty(value))
        {
            errorMessage = "File name cannot be null or empty";
            return false;
        }
        
        if (value.Any(c => InvalidChars.Contains(c)))
        {
            errorMessage = "File name contains invalid characters";
            return false;
        }
        
        if (value.Trim() != value)
        {
            errorMessage = "File name cannot start or end with whitespace";
            return false;
        }
        
        if (value.Length > 255)
        {
            errorMessage = "File name exceeds maximum length of 255 characters";
            return false;
        }
        
        errorMessage = null;
        return true;
    }
}