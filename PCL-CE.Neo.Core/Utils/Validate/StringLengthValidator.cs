namespace PCL_CE.Neo.Core.Utils.Validate;

public class StringLengthValidator
{
    private readonly int _minLength;
    private readonly int _maxLength;

    public StringLengthValidator(int maxLength)
    {
        _minLength = 0;
        _maxLength = maxLength;
    }

    public StringLengthValidator(int minLength, int maxLength)
    {
        _minLength = minLength;
        _maxLength = maxLength;
    }

    public bool Validate(string? input)
    {
        if (input == null)
            return _minLength == 0;

        return input.Length >= _minLength && input.Length <= _maxLength;
    }
}