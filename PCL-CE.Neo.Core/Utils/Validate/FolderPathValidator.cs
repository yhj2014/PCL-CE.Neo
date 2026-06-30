using System;
using System.IO;

namespace PCL_CE.Neo.Core.Utils.Validate;

public class FolderPathValidator : ValidatorBase<string?>
{
    public override bool Validate(string? value, out string? errorMessage)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            errorMessage = "Folder path cannot be null or empty";
            return false;
        }
        
        try
        {
            _ = Path.GetFullPath(value);
        }
        catch (Exception)
        {
            errorMessage = "Invalid folder path format";
            return false;
        }
        
        errorMessage = null;
        return true;
    }
}