using System;
using System.Collections.Generic;
using System.Linq;

namespace PCL_CE.Neo.Core.Utils;

public static class UriHelper
{
    public static Uri Combine(params string[] parts)
    {
        if (parts == null || parts.Length == 0)
            throw new ArgumentNullException(nameof(parts));

        string combined = parts[0];

        for (int i = 1; i < parts.Length; i++)
        {
            combined = Uri.TryCreate(new Uri(combined), parts[i], out var result) 
                ? result.ToString() 
                : System.IO.Path.Combine(combined, parts[i]);
        }

        return new Uri(combined);
    }

    public static string CombinePath(params string[] parts)
    {
        if (parts == null || parts.Length == 0)
            throw new ArgumentNullException(nameof(parts));

        string combined = parts[0];

        for (int i = 1; i < parts.Length; i++)
        {
            combined = Uri.TryCreate(new Uri(combined + "/"), parts[i], out var result)
                ? result.ToString()
                : combined.TrimEnd('/') + "/" + parts[i].TrimStart('/');
        }

        return combined;
    }

    public static bool IsValidUri(string uriString)
    {
        return Uri.TryCreate(uriString, UriKind.Absolute, out _);
    }

    public static bool IsValidUrl(string urlString)
    {
        if (!Uri.TryCreate(urlString, UriKind.Absolute, out var uri))
            return false;

        return uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps;
    }

    public static bool IsValidHttpUrl(string urlString)
    {
        return IsValidUrl(urlString) && Uri.TryCreate(urlString, UriKind.Absolute, out var uri) && 
               (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }

    public static bool IsValidHttpsUrl(string urlString)
    {
        return IsValidUrl(urlString) && Uri.TryCreate(urlString, UriKind.Absolute, out var uri) && 
               uri.Scheme == Uri.UriSchemeHttps;
    }

    public static string GetScheme(string uriString)
    {
        if (!Uri.TryCreate(uriString, UriKind.Absolute, out var uri))
            return string.Empty;

        return uri.Scheme;
    }

    public static string GetHost(string uriString)
    {
        if (!Uri.TryCreate(uriString, UriKind.Absolute, out var uri))
            return string.Empty;

        return uri.Host;
    }

    public static int GetPort(string uriString)
    {
        if (!Uri.TryCreate(uriString, UriKind.Absolute, out var uri))
            return -1;

        return uri.Port;
    }

    public static string GetPath(string uriString)
    {
        if (!Uri.TryCreate(uriString, UriKind.Absolute, out var uri))
            return string.Empty;

        return uri.AbsolutePath;
    }

    public static string GetQuery(string uriString)
    {
        if (!Uri.TryCreate(uriString, UriKind.Absolute, out var uri))
            return string.Empty;

        return uri.Query;
    }

    public static string GetFragment(string uriString)
    {
        if (!Uri.TryCreate(uriString, UriKind.Absolute, out var uri))
            return string.Empty;

        return uri.Fragment;
    }

    public static Dictionary<string, string> ParseQuery(string uriString)
    {
        var query = GetQuery(uriString).TrimStart('?');
        var parameters = new Dictionary<string, string>();

        if (string.IsNullOrEmpty(query))
            return parameters;

        foreach (var pair in query.Split('&'))
        {
            var parts = pair.Split('=');
            if (parts.Length >= 1)
            {
                var key = Uri.UnescapeDataString(parts[0]);
                var value = parts.Length >= 2 ? Uri.UnescapeDataString(parts[1]) : string.Empty;
                parameters[key] = value;
            }
        }

        return parameters;
    }

    public static string AddQueryParameter(string uriString, string key, string value)
    {
        if (!Uri.TryCreate(uriString, UriKind.Absolute, out var uri))
            return uriString;

        var query = ParseQuery(uriString);
        query[key] = value;

        return BuildUri(uri.Scheme, uri.Host, uri.Port, uri.AbsolutePath, query);
    }

    public static string RemoveQueryParameter(string uriString, string key)
    {
        if (!Uri.TryCreate(uriString, UriKind.Absolute, out var uri))
            return uriString;

        var query = ParseQuery(uriString);
        query.Remove(key);

        return BuildUri(uri.Scheme, uri.Host, uri.Port, uri.AbsolutePath, query);
    }

    public static string BuildUri(string scheme, string host, int port, string path, Dictionary<string, string>? query = null)
    {
        var uriBuilder = new UriBuilder
        {
            Scheme = scheme,
            Host = host,
            Port = port,
            Path = path
        };

        if (query != null && query.Count > 0)
        {
            uriBuilder.Query = string.Join("&", query.Select(kvp => 
                $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));
        }

        return uriBuilder.Uri.ToString();
    }

    public static string NormalizeUrl(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            return url;

        return new UriBuilder(uri)
        {
            Path = uri.AbsolutePath.TrimEnd('/'),
            Query = string.Empty,
            Fragment = string.Empty
        }.Uri.ToString();
    }

    public static string EnsureTrailingSlash(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            return url.EndsWith('/') ? url : url + '/';

        if (uri.AbsolutePath.EndsWith('/'))
            return uri.ToString();

        return new UriBuilder(uri) { Path = uri.AbsolutePath + '/' }.Uri.ToString();
    }

    public static string RemoveTrailingSlash(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            return url.TrimEnd('/');

        if (!uri.AbsolutePath.EndsWith('/'))
            return uri.ToString();

        return new UriBuilder(uri) { Path = uri.AbsolutePath.TrimEnd('/') }.Uri.ToString();
    }

    public static string EnsureScheme(string url, string defaultScheme = "https")
    {
        if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
            return uri.ToString();

        return $"{defaultScheme}://{url.TrimStart('/')}";
    }

    public static bool IsLocalhost(string uriString)
    {
        if (!Uri.TryCreate(uriString, UriKind.Absolute, out var uri))
            return false;

        var host = uri.Host.ToLowerInvariant();
        return host == "localhost" || host == "127.0.0.1" || host == "::1";
    }

    public static bool IsLocalNetwork(string uriString)
    {
        if (!Uri.TryCreate(uriString, UriKind.Absolute, out var uri))
            return false;

        var host = uri.Host.ToLowerInvariant();
        if (host == "localhost" || host == "127.0.0.1" || host == "::1")
            return true;

        if (System.Net.IPAddress.TryParse(host, out var ipAddress))
        {
            return IsPrivateAddress(ipAddress);
        }

        return false;
    }

    private static bool IsPrivateAddress(System.Net.IPAddress ipAddress)
    {
        if (ipAddress == null)
            return false;

        byte[] bytes = ipAddress.GetAddressBytes();

        return bytes[0] == 10 ||
               (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31) ||
               (bytes[0] == 192 && bytes[1] == 168) ||
               (bytes[0] == 169 && bytes[1] == 254) ||
               (bytes[0] == 127);
    }
}