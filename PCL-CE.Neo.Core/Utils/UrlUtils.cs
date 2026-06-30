using System;
using System.Collections.Generic;
using System.Linq;

namespace PCL_CE.Neo.Core.Utils;

public static class UrlUtils
{
    public static string Encode(string url)
    {
        if (string.IsNullOrEmpty(url))
            return url ?? string.Empty;

        return Uri.EscapeDataString(url);
    }

    public static string Decode(string url)
    {
        if (string.IsNullOrEmpty(url))
            return url ?? string.Empty;

        return Uri.UnescapeDataString(url);
    }

    public static string EncodePath(string path)
    {
        if (string.IsNullOrEmpty(path))
            return path ?? string.Empty;

        var segments = path.Split('/');
        var encodedSegments = segments.Select(s => Uri.EscapeDataString(s));
        return string.Join("/", encodedSegments);
    }

    public static string DecodePath(string path)
    {
        if (string.IsNullOrEmpty(path))
            return path ?? string.Empty;

        var segments = path.Split('/');
        var decodedSegments = segments.Select(s => Uri.UnescapeDataString(s));
        return string.Join("/", decodedSegments);
    }

    public static string AddParameter(string url, string name, string value)
    {
        if (string.IsNullOrEmpty(url))
            return url ?? string.Empty;

        var separator = url.Contains('?') ? '&' : '?';
        return $"{url}{separator}{Uri.EscapeDataString(name)}={Uri.EscapeDataString(value)}";
    }

    public static string AddParameters(string url, IDictionary<string, string> parameters)
    {
        if (string.IsNullOrEmpty(url))
            return url ?? string.Empty;

        if (parameters == null || parameters.Count == 0)
            return url;

        var separator = url.Contains('?') ? '&' : '?';
        var queryString = string.Join("&", parameters.Select(kvp =>
            $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));

        return $"{url}{separator}{queryString}";
    }

    public static string RemoveParameter(string url, string name)
    {
        if (string.IsNullOrEmpty(url))
            return url ?? string.Empty;

        var uri = new Uri(url);
        var query = uri.Query.TrimStart('?');

        if (string.IsNullOrEmpty(query))
            return url;

        var parameters = query.Split('&')
            .Where(p => !p.StartsWith($"{Uri.EscapeDataString(name)}=", StringComparison.Ordinal))
            .ToList();

        var newQuery = string.Join("&", parameters);
        var newUri = new UriBuilder(uri) { Query = newQuery };

        return newUri.Uri.ToString();
    }

    public static string GetParameter(string url, string name)
    {
        if (string.IsNullOrEmpty(url))
            return string.Empty;

        var uri = new Uri(url);
        var query = uri.Query.TrimStart('?');

        if (string.IsNullOrEmpty(query))
            return string.Empty;

        foreach (var parameter in query.Split('&'))
        {
            var parts = parameter.Split('=');
            if (parts.Length >= 1 && parts[0] == Uri.EscapeDataString(name))
            {
                return parts.Length >= 2 ? Uri.UnescapeDataString(parts[1]) : string.Empty;
            }
        }

        return string.Empty;
    }

    public static bool HasParameter(string url, string name)
    {
        if (string.IsNullOrEmpty(url))
            return false;

        var uri = new Uri(url);
        var query = uri.Query.TrimStart('?');

        if (string.IsNullOrEmpty(query))
            return false;

        foreach (var parameter in query.Split('&'))
        {
            var parts = parameter.Split('=');
            if (parts.Length >= 1 && parts[0] == Uri.EscapeDataString(name))
            {
                return true;
            }
        }

        return false;
    }

    public static IDictionary<string, string> GetParameters(string url)
    {
        var parameters = new Dictionary<string, string>();

        if (string.IsNullOrEmpty(url))
            return parameters;

        var uri = new Uri(url);
        var query = uri.Query.TrimStart('?');

        if (string.IsNullOrEmpty(query))
            return parameters;

        foreach (var parameter in query.Split('&'))
        {
            var parts = parameter.Split('=');
            if (parts.Length >= 1)
            {
                var name = Uri.UnescapeDataString(parts[0]);
                var value = parts.Length >= 2 ? Uri.UnescapeDataString(parts[1]) : string.Empty;
                parameters[name] = value;
            }
        }

        return parameters;
    }

    public static string ReplaceParameter(string url, string name, string value)
    {
        if (string.IsNullOrEmpty(url))
            return url ?? string.Empty;

        url = RemoveParameter(url, name);
        return AddParameter(url, name, value);
    }

    public static string BuildQueryString(IDictionary<string, string> parameters)
    {
        if (parameters == null || parameters.Count == 0)
            return string.Empty;

        return string.Join("&", parameters.Select(kvp =>
            $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));
    }

    public static IDictionary<string, string> ParseQueryString(string queryString)
    {
        var parameters = new Dictionary<string, string>();

        if (string.IsNullOrEmpty(queryString))
            return parameters;

        var trimmed = queryString.TrimStart('?');

        foreach (var parameter in trimmed.Split('&'))
        {
            var parts = parameter.Split('=');
            if (parts.Length >= 1)
            {
                var name = Uri.UnescapeDataString(parts[0]);
                var value = parts.Length >= 2 ? Uri.UnescapeDataString(parts[1]) : string.Empty;
                parameters[name] = value;
            }
        }

        return parameters;
    }

    public static string ExtractDomain(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            return string.Empty;

        return uri.Host;
    }

    public static string ExtractPath(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            return string.Empty;

        return uri.AbsolutePath;
    }

    public static string ExtractProtocol(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            return string.Empty;

        return uri.Scheme;
    }

    public static string RemoveQuery(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            return url ?? string.Empty;

        return new UriBuilder(uri) { Query = string.Empty }.Uri.ToString();
    }

    public static string RemoveFragment(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            return url ?? string.Empty;

        return new UriBuilder(uri) { Fragment = string.Empty }.Uri.ToString();
    }

    public static string RemoveQueryAndFragment(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            return url ?? string.Empty;

        return new UriBuilder(uri) { Query = string.Empty, Fragment = string.Empty }.Uri.ToString();
    }
}