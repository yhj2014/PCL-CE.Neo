using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace PCL_CE.Neo.Core.Utils.Network;

public static class NetworkUtils
{
    public static bool IsNetworkAvailable()
    {
        return NetworkInterface.GetIsNetworkAvailable();
    }

    public static IEnumerable<IPAddress> GetLocalIpAddresses()
    {
        List<IPAddress> addresses = new List<IPAddress>();

        foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (ni.OperationalStatus != OperationalStatus.Up)
                continue;

            foreach (UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses)
            {
                if (ip.Address.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(ip.Address))
                    addresses.Add(ip.Address);
            }
        }

        return addresses;
    }

    public static IPAddress? GetPrimaryLocalIpAddress()
    {
        using Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        try
        {
            socket.Connect("8.8.8.8", 65530);
            IPEndPoint? endPoint = socket.LocalEndPoint as IPEndPoint;
            return endPoint?.Address;
        }
        catch
        {
            return GetLocalIpAddresses().FirstOrDefault();
        }
    }

    public static bool IsPortAvailable(int port)
    {
        try
        {
            using TcpListener listener = new TcpListener(IPAddress.Any, port);
            listener.Start();
            listener.Stop();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static int FindAvailablePort(int startPort = 1024, int endPort = 65535)
    {
        for (int port = startPort; port <= endPort; port++)
        {
            if (IsPortAvailable(port))
                return port;
        }
        throw new Exception("没有找到可用的端口。");
    }

    public static async Task<bool> TestConnectionAsync(string host, int port, int timeoutMs = 5000)
    {
        try
        {
            using TcpClient client = new TcpClient();
            Task connectTask = client.ConnectAsync(host, port);
            if (await Task.WhenAny(connectTask, Task.Delay(timeoutMs)) == connectTask)
            {
                return client.Connected;
            }
            return false;
        }
        catch
        {
            return false;
        }
    }

    public static bool TestConnection(string host, int port, int timeoutMs = 5000)
    {
        try
        {
            using TcpClient client = new TcpClient();
            IAsyncResult result = client.BeginConnect(host, port, null, null);
            bool success = result.AsyncWaitHandle.WaitOne(timeoutMs);
            if (success)
                client.EndConnect(result);
            return client.Connected;
        }
        catch
        {
            return false;
        }
    }

    public static async Task<long?> PingAsync(string host, int timeoutMs = 3000)
    {
        try
        {
            using Ping ping = new Ping();
            PingReply reply = await ping.SendPingAsync(host, timeoutMs);
            return reply.Status == IPStatus.Success ? reply.RoundtripTime : null;
        }
        catch
        {
            return null;
        }
    }

    public static long? Ping(string host, int timeoutMs = 3000)
    {
        try
        {
            using Ping ping = new Ping();
            PingReply reply = ping.Send(host, timeoutMs);
            return reply.Status == IPStatus.Success ? reply.RoundtripTime : null;
        }
        catch
        {
            return null;
        }
    }

    public static async Task<string?> ResolveHostAsync(string host)
    {
        try
        {
            IPAddress[] addresses = await Dns.GetHostAddressesAsync(host);
            return addresses.FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork)?.ToString();
        }
        catch
        {
            return null;
        }
    }

    public static string? ResolveHost(string host)
    {
        try
        {
            IPAddress[] addresses = Dns.GetHostAddresses(host);
            return addresses.FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork)?.ToString();
        }
        catch
        {
            return null;
        }
    }

    public static string GetHostName()
    {
        return Dns.GetHostName();
    }

    public static bool IsPrivateIpAddress(IPAddress ip)
    {
        byte[] bytes = ip.GetAddressBytes();
        if (bytes[0] == 10)
            return true;
        if (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31)
            return true;
        if (bytes[0] == 192 && bytes[1] == 168)
            return true;
        return false;
    }

    public static bool IsPublicIpAddress(IPAddress ip)
    {
        return !IsPrivateIpAddress(ip) && !IPAddress.IsLoopback(ip);
    }

    public static bool IsValidIpAddress(string address)
    {
        return IPAddress.TryParse(address, out _);
    }

    public static bool IsValidIpv4Address(string address)
    {
        if (!IPAddress.TryParse(address, out IPAddress? ip))
            return false;
        return ip.AddressFamily == AddressFamily.InterNetwork;
    }

    public static bool IsValidIpv6Address(string address)
    {
        if (!IPAddress.TryParse(address, out IPAddress? ip))
            return false;
        return ip.AddressFamily == AddressFamily.InterNetworkV6;
    }

    public static string FormatIpAddress(IPAddress ip)
    {
        if (ip.AddressFamily == AddressFamily.InterNetworkV6)
            return $"[{ip}]";
        return ip.ToString();
    }

    public static async Task<string?> GetPublicIpAddressAsync()
    {
        string[] endpoints = {
            "https://api.ipify.org",
            "https://icanhazip.com",
            "https://ifconfig.me/ip",
            "https://api.my-ip.io/ip"
        };

        foreach (string endpoint in endpoints)
        {
            try
            {
                using HttpClient client = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
                string ip = await client.GetStringAsync(endpoint);
                ip = ip.Trim();
                if (IsValidIpAddress(ip))
                    return ip;
            }
            catch
            {
            }
        }

        return null;
    }

    public static string? GetPublicIpAddress()
    {
        return GetPublicIpAddressAsync().GetAwaiter().GetResult();
    }
}