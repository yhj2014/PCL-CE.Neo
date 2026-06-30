using System;

namespace PCL_CE.Neo.Core.Utils.Validate;

public class NullOrWhiteSpaceValidator : ValidatorBase<string?>
{
    public override bool Validate(string? value, out string? errorMessage)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            errorMessage = "Value cannot be null or whitespace";
            return false;
        }
        
        errorMessage = null;
        return true;
    }
}