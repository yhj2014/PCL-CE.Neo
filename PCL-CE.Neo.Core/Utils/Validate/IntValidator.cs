using System;
using Microsoft.Extensions.Logging;

namespace PCL_CE.Neo.Core.Utils.Validate;

public class IntValidator : IValidator<int>
{
    private readonly int? _minValue;
    private readonly int? _maxValue;
    private readonly ILogger<IntValidator> _logger;

    public IntValidator(ILogger<IntValidator> logger)
    {
        _logger = logger;
    }

    public IntValidator(int minValue, int maxValue, ILogger<IntValidator> logger)
    {
        if (minValue > maxValue)
            throw new ArgumentException("Min value cannot be greater than max value");

        _minValue = minValue;
        _maxValue = maxValue;
        _logger = logger;
    }

    public IntValidator(int minValue, ILogger<IntValidator> logger)
    {
        _minValue = minValue;
        _logger = logger;
    }

    public bool Validate(int value, out string? errorMessage)
    {
        errorMessage = null;

        try
        {
            if (_minValue.HasValue && value < _minValue.Value)
            {
                _logger.LogDebug("Input {Input} is less than min value {MinValue}", value, _minValue.Value);
                errorMessage = GetErrorMessage(value);
                return false;
            }

            if (_maxValue.HasValue && value > _maxValue.Value)
            {
                _logger.LogDebug("Input {Input} is greater than max value {MaxValue}", value, _maxValue.Value);
                errorMessage = GetErrorMessage(value);
                return false;
            }

            _logger.LogDebug("Int validation passed for input: {Input}", value);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Int validation failed with exception");
            errorMessage = "Validation failed";
            return false;
        }
    }

    public string GetErrorMessage(int input)
    {
        if (_minValue.HasValue && _maxValue.HasValue)
            return $"Value must be between {_minValue.Value} and {_maxValue.Value}";

        if (_minValue.HasValue)
            return $"Value must be greater than or equal to {_minValue.Value}";

        if (_maxValue.HasValue)
            return $"Value must be less than or equal to {_maxValue.Value}";

        return "Invalid value";
    }

    public int? MinValue => _minValue;
    public int? MaxValue => _maxValue;
}