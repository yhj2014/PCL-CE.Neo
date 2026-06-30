using System;

namespace PCL_CE.Neo.Core.Utils.Validate;

public class StringLengthValidator : ValidatorBase<string?>
{
    private readonly int _minLength;
    private readonly int _maxLength;
    
    public StringLengthValidator(int minLength = 0, int maxLength = int.MaxValue)
    {
        _minLength = minLength;
        _maxLength = maxLength;
    }
    
    public override bool Validate(string? value, out string? errorMessage)
    {
        if (value == null)
        {
            errorMessage = "Value cannot be null";
            return false;
        }
        
        if (value.Length < _minLength)
        {
            errorMessage = $"Value must be at least {_minLength} characters long";
            return false;
        }
        
        if (value.Length > _maxLength)
        {
            errorMessage = $"Value must be at most {_maxLength} characters long";
            return false;
        }
        
        errorMessage = null;
        return true;
    }
}