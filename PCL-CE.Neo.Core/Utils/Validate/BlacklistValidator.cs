using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace PCL_CE.Neo.Core.Utils.Validate;

public class BlacklistValidator : IValidator<string>
{
    private readonly HashSet<string> _blacklist;
    private readonly ILogger<BlacklistValidator> _logger;

    public BlacklistValidator(IEnumerable<string> blacklist, ILogger<BlacklistValidator> logger)
    {
        _blacklist = new HashSet<string>(blacklist ?? throw new ArgumentNullException(nameof(blacklist)), StringComparer.OrdinalIgnoreCase);
        _logger = logger;
    }

    public bool Validate(string? value, out string? errorMessage)
    {
        errorMessage = null;

        try
        {
            if (value == null)
            {
                _logger.LogDebug("Input is null, validation passed");
                return true;
            }

            var isBlacklisted = _blacklist.Contains(value);
            if (isBlacklisted)
            {
                _logger.LogDebug("Input '{Input}' is in blacklist", value);
                errorMessage = GetErrorMessage(value);
                return false;
            }

            _logger.LogDebug("Blacklist validation passed for input: {Input}", value);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Blacklist validation failed with exception");
            errorMessage = "Validation failed";
            return false;
        }
    }

    public string GetErrorMessage(string? input)
    {
        return $"Value '{input}' is not allowed";
    }

    public bool IsBlacklisted(string input)
    {
        return _blacklist.Contains(input);
    }

    public int BlacklistCount => _blacklist.Count;
}