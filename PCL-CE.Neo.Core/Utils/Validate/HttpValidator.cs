using System;
using System.Text.RegularExpressions;

namespace PCL_CE.Neo.Core.Utils.Validate;

public class HttpValidator : ValidatorBase<string?>
{
    private static readonly Regex HttpRegex = new(
        @"^(https?):\/\/[^\s/$.?#].[^\s]*$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );
    
    public override bool Validate(string? value, out string? errorMessage)
    {
        if (string.IsNullOrEmpty(value))
        {
            errorMessage = "URL cannot be null or empty";
            return false;
        }
        
        if (!HttpRegex.IsMatch(value))
        {
            errorMessage = "Invalid HTTP/HTTPS URL format";
            return false;
        }
        
        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri))
        {
            errorMessage = "Invalid URL";
            return false;
        }
        
        if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
        {
            errorMessage = "URL must be HTTP or HTTPS";
            return false;
        }
        
        errorMessage = null;
        return true;
    }
}