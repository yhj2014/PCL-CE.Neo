using System;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace PCL_CE.Neo.Core.Utils.Validate;

public class RegexValidator : IValidator<string>
{
    private readonly Regex _regex;
    private readonly ILogger<RegexValidator> _logger;

    public RegexValidator(string pattern, ILogger<RegexValidator> logger)
        : this(new Regex(pattern), logger)
    {
    }

    public RegexValidator(Regex regex, ILogger<RegexValidator> logger)
    {
        _regex = regex ?? throw new ArgumentNullException(nameof(regex));
        _logger = logger;
    }

    public bool Validate(string? value, out string? errorMessage)
    {
        errorMessage = null;

        try
        {
            if (value == null)
            {
                _logger.LogDebug("Input is null, validation failed");
                errorMessage = "Input cannot be null";
                return false;
            }

            var isValid = _regex.IsMatch(value);
            if (!isValid)
            {
                errorMessage = GetErrorMessage(value);
            }
            _logger.LogDebug("Regex validation {Result} for input: {Input}", isValid ? "passed" : "failed", value);
            return isValid;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Regex validation failed with exception");
            errorMessage = "Validation failed";
            return false;
        }
    }

    public string GetErrorMessage(string? input)
    {
        return "Input does not match the required pattern";
    }

    public string Pattern => _regex.ToString();
}