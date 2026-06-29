using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace PCL_CE.Neo.Core.Link;

public static class ServerAddressResolver
{
    private static readonly Regex _addressRegex = new(
        @"^(?<host>[^:\s]+)(?::(?<port>\d{1,5}))?$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public static (string Host, int Port) ParseAddress(string address)
    {
        if (string.IsNullOrWhiteSpace(address))
            return ("localhost", 25565);

        var match = _addressRegex.Match(address.Trim());
        if (!match.Success)
            return (address.Trim(), 25565);

        var host = match.Groups["host"].Value;
        var port = match.Groups["port"].Success && int.TryParse(match.Groups["port"].Value, out var parsedPort)
            ? parsedPort
            : 25565;

        if (port < 1 || port > 65535)
            port = 25565;

        return (host, port);
    }

    public static string FormatAddress(string host, int port)
    {
        return port == 25565 ? host : $"{host}:{port}";
    }

    public static bool IsValidAddress(string address)
    {
        if (string.IsNullOrWhiteSpace(address))
            return false;

        var match = _addressRegex.Match(address.Trim());
        if (!match.Success)
            return false;

        var host = match.Groups["host"].Value;
        if (string.IsNullOrWhiteSpace(host))
            return false;

        if (match.Groups["port"].Success)
        {
            if (!int.TryParse(match.Groups["port"].Value, out var port))
                return false;
            if (port < 1 || port > 65535)
                return false;
        }

        return true;
    }

    public static bool IsValidHost(string host)
    {
        if (string.IsNullOrWhiteSpace(host))
            return false;

        if (host.Length > 253)
            return false;

        var parts = host.Split('.');
        if (parts.Length == 0)
            return false;

        foreach (var part in parts)
        {
            if (string.IsNullOrEmpty(part) || part.Length > 63)
                return false;

            if (!IsValidHostPart(part))
                return false;
        }

        return true;
    }

    private static bool IsValidHostPart(string part)
    {
        if (string.IsNullOrEmpty(part))
            return false;

        foreach (var c in part)
        {
            if (!char.IsLetterOrDigit(c) && c != '-')
                return false;
        }

        if (part.StartsWith('-') || part.EndsWith('-'))
            return false;

        return true;
    }

    public static bool IsValidPort(int port)
    {
        return port >= 1 && port <= 65535;
    }
}