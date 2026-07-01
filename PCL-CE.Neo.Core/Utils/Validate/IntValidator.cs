namespace PCL_CE.Neo.Core.Utils.Validate;

public class IntValidator
{
    public int MinValue { get; }
    public int MaxValue { get; }

    public IntValidator(int minValue, int maxValue)
    {
        if (minValue > maxValue)
            throw new ArgumentOutOfRangeException(nameof(maxValue), "Maximum value cannot be less than minimum value.");

        MinValue = minValue;
        MaxValue = maxValue;
    }

    public bool Validate(int value)
    {
        return value >= MinValue && value <= MaxValue;
    }

    public bool Validate(int value, out string? errorMessage)
    {
        errorMessage = null;

        if (value < MinValue)
        {
            errorMessage = $"Value must be greater than or equal to {MinValue}.";
            return false;
        }

        if (value > MaxValue)
        {
            errorMessage = $"Value must be less than or equal to {MaxValue}.";
            return false;
        }

        return true;
    }

    public void ValidateAndThrow(int value, string? paramName = null)
    {
        if (!Validate(value, out var errorMessage))
            throw new ArgumentOutOfRangeException(paramName, errorMessage);
    }
}