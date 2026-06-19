namespace PCL_CE.Neo.Core.Utils.Validate;

public class IntValidator
{
    private readonly int? _minValue;
    private readonly int? _maxValue;

    public IntValidator()
    {
    }

    public IntValidator(int minValue, int maxValue)
    {
        _minValue = minValue;
        _maxValue = maxValue;
    }

    public bool Validate(int value)
    {
        if (_minValue.HasValue && value < _minValue.Value)
            return false;
        if (_maxValue.HasValue && value > _maxValue.Value)
            return false;
        return true;
    }

    public bool Validate(string? input)
    {
        if (!int.TryParse(input, out var value))
            return false;
        return Validate(value);
    }
}