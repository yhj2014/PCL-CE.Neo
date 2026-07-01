namespace PCL_CE.Neo.Core.Utils.Validate;

public class StringLengthValidator : IValidator<string>
{
    private readonly int _minLength;
    private readonly int _maxLength;

    public StringLengthValidator(int minLength, int maxLength)
    {
        if (minLength < 0)
            throw new ArgumentOutOfRangeException(nameof(minLength), "最小长度不能为负数。");
        if (maxLength < minLength)
            throw new ArgumentOutOfRangeException(nameof(maxLength), "最大长度不能小于最小长度。");

        _minLength = minLength;
        _maxLength = maxLength;
    }

    public bool Validate(string? value)
    {
        if (value == null)
            return _minLength == 0;

        return value.Length >= _minLength && value.Length <= _maxLength;
    }

    public string GetErrorMessage(string? value)
    {
        if (value == null)
        {
            return _minLength == 0 
                ? string.Empty 
                : "值不能为null";
        }

        if (value.Length < _minLength)
            return $"值长度 {value.Length} 小于最小长度 {_minLength}";

        if (value.Length > _maxLength)
            return $"值长度 {value.Length} 大于最大长度 {_maxLength}";

        return string.Empty;
    }
}