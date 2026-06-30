using System;

namespace PCL_CE.Neo.Core.Utils.Validate;

public class NullOrEmptyValidator : ValidatorBase<string?>
{
    public override bool Validate(string? value, out string? errorMessage)
    {
        if (string.IsNullOrEmpty(value))
        {
            errorMessage = "Value cannot be null or empty";
            return false;
        }
        
        errorMessage = null;
        return true;
    }
}