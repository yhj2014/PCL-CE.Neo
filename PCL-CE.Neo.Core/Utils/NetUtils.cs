using System;
using System.Net;
using System.Net.Sockets;

namespace PCL_CE.Neo.Core.Utils;

public static class NetUtils
{
    public static Task<bool> IsPortAvailableAsync(int port)
    {
        return Task.Run(() => IsPortAvailable(port));
    }

    public static bool IsPortAvailable(int port)
    {
        if (port < 1 || port > 65535)
            throw new ArgumentOutOfRangeException(nameof(port), "Port must be between 1 and 65535");

        try
        {
            using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(new IPEndPoint(IPAddress.Loopback, port));
            return true;
        }
        catch (SocketException)
        {
            return false;
        }
    }

    public static int GetAvailablePort()
    {
        for (int port = 1024; port <= 65535; port++)
        {
            if (IsPortAvailable(port))
                return port;
        }
        throw new InvalidOperationException("No available port found");
    }

    public static Task<int> GetAvailablePortAsync()
    {
        return Task.Run(() => GetAvailablePort());
    }

    public static bool IsValidPort(int port)
    {
        return port >= 1 && port <= 65535;
    }

    public static bool IsValidIPAddress(string ipAddress)
    {
        if (string.IsNullOrWhiteSpace(ipAddress))
            return false;

        return IPAddress.TryParse(ipAddress, out _);
    }

    public static bool IsValidIPv4Address(string ipAddress)
    {
        if (string.IsNullOrWhiteSpace(ipAddress))
            return false;

        if (!IPAddress.TryParse(ipAddress, out var ip))
            return false;

        return ip.AddressFamily == AddressFamily.InterNetwork;
    }

    public static bool IsValidIPv6Address(string ipAddress)
    {
        if (string.IsNullOrWhiteSpace(ipAddress))
            return false;

        if (!IPAddress.TryParse(ipAddress, out var ip))
            return false;

        return ip.AddressFamily == AddressFamily.InterNetworkV6;
    }

    public static bool IsPrivateIPAddress(string ipAddress)
    {
        if (!IPAddress.TryParse(ipAddress, out var ip))
            return false;

        byte[] bytes = ip.GetAddressBytes();

        if (bytes[0] == 10)
            return true;

        if (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31)
            return true;

        if (bytes[0] == 192 && bytes[1] == 168)
            return true;

        if (bytes[0] == 169 && bytes[1] == 254)
            return true;

        return false;
    }

    public static string GetLocalIPAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }
        throw new InvalidOperationException("No local IP address found");
    }

    public static Task<string> GetLocalIPAddressAsync()
    {
        return Task.Run(() => GetLocalIPAddress());
    }

    public static Task<bool> CanConnectAsync(string host, int port, int timeoutMs = 5000)
    {
        return Task.Run(() => CanConnect(host, port, timeoutMs));
    }

    public static bool CanConnect(string host, int port, int timeoutMs = 5000)
    {
        if (string.IsNullOrWhiteSpace(host))
            throw new ArgumentNullException(nameof(host));
        if (!IsValidPort(port))
            throw new ArgumentOutOfRangeException(nameof(port));

        try
        {
            using var tcpClient = new TcpClient();
            tcpClient.Connect(host, port);
            return tcpClient.Connected;
        }
        catch (SocketException)
        {
            return false;
        }
        catch (TimeoutException)
        {
            return false;
        }
    }
}