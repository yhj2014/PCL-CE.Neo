namespace PCL_CE.Neo.Core.Utils.Validate;

public class HttpValidator
{
    public static bool IsValidUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return false;

        return Uri.TryCreate(url, UriKind.Absolute, out var uri) &&
               (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }

    public static bool IsValidUrl(string? url, out string? errorMessage)
    {
        errorMessage = null;

        if (string.IsNullOrWhiteSpace(url))
        {
            errorMessage = "URL cannot be null or empty.";
            return false;
        }

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            errorMessage = "Invalid URL format.";
            return false;
        }

        if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
        {
            errorMessage = "URL must use HTTP or HTTPS scheme.";
            return false;
        }

        return true;
    }

    public static bool IsValidHost(string? host)
    {
        if (string.IsNullOrWhiteSpace(host))
            return false;

        if (Uri.TryCreate($"http://{host}", UriKind.Absolute, out var uri))
            return !string.IsNullOrWhiteSpace(uri.Host);

        return false;
    }

    public static bool IsValidPort(int port)
    {
        return port >= 1 && port <= 65535;
    }

    public static void ValidateUrlAndThrow(string? url, string? paramName = null)
    {
        if (!IsValidUrl(url, out var errorMessage))
            throw new ArgumentException(errorMessage, paramName);
    }

    public static void ValidatePortAndThrow(int port, string? paramName = null)
    {
        if (!IsValidPort(port))
            throw new ArgumentOutOfRangeException(paramName, "Port must be between 1 and 65535.");
    }
}