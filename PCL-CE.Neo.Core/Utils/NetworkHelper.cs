using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;

namespace PCL_CE.Neo.Core.Utils;

public static class NetworkHelper
{
    public static string GetLocalIpAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }

        return "127.0.0.1";
    }

    public static string[] GetLocalIpAddresses()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        return host.AddressList
            .Where(ip => ip.AddressFamily == AddressFamily.InterNetwork)
            .Select(ip => ip.ToString())
            .ToArray();
    }

    public static bool IsLocalIpAddress(string ipAddress)
    {
        if (!IPAddress.TryParse(ipAddress, out var ip))
            return false;

        return IPAddress.IsLoopback(ip) ||
               ip.AddressFamily == AddressFamily.InterNetwork &&
               ((ip.GetAddressBytes()[0] == 10) ||
                (ip.GetAddressBytes()[0] == 172 && ip.GetAddressBytes()[1] >= 16 && ip.GetAddressBytes()[1] <= 31) ||
                (ip.GetAddressBytes()[0] == 192 && ip.GetAddressBytes()[1] == 168));
    }

    public static bool IsPortAvailable(int port)
    {
        try
        {
            using var listener = new TcpListener(IPAddress.Loopback, port);
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

        throw new InvalidOperationException("未找到可用端口");
    }

    public static async Task<bool> TestConnectionAsync(string host, int port, int timeout = 5000)
    {
        try
        {
            using var tcpClient = new TcpClient();
            var connectTask = tcpClient.ConnectAsync(host, port);
            var timeoutTask = Task.Delay(timeout);

            var completedTask = await Task.WhenAny(connectTask, timeoutTask);
            if (completedTask == timeoutTask)
                return false;

            await connectTask;
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static bool TestConnection(string host, int port, int timeout = 5000)
    {
        return TestConnectionAsync(host, port, timeout).GetAwaiter().GetResult();
    }

    public static async Task<bool> TestHttpConnectionAsync(string url, int timeout = 10000)
    {
        try
        {
            using var httpClient = new HttpClient { Timeout = TimeSpan.FromMilliseconds(timeout) };
            var response = await httpClient.GetAsync(url);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public static bool TestHttpConnection(string url, int timeout = 10000)
    {
        return TestHttpConnectionAsync(url, timeout).GetAwaiter().GetResult();
    }

    public static async Task<long?> GetPingTimeAsync(string host, int timeout = 5000)
    {
        try
        {
            using var ping = new Ping();
            var reply = await ping.SendPingAsync(host, timeout);

            if (reply.Status == IPStatus.Success)
                return reply.RoundtripTime;

            return null;
        }
        catch
        {
            return null;
        }
    }

    public static long? GetPingTime(string host, int timeout = 5000)
    {
        return GetPingTimeAsync(host, timeout).GetAwaiter().GetResult();
    }

    public static string GetHostName()
    {
        return Dns.GetHostName();
    }

    public static string GetFullyQualifiedDomainName()
    {
        return Dns.GetHostEntry(Dns.GetHostName()).HostName;
    }

    public static bool IsNetworkAvailable()
    {
        try
        {
            return NetworkInterface.GetIsNetworkAvailable();
        }
        catch
        {
            return false;
        }
    }

    public static NetworkInterface[] GetActiveNetworkInterfaces()
    {
        try
        {
            return NetworkInterface.GetAllNetworkInterfaces()
                .Where(n => n.OperationalStatus == OperationalStatus.Up)
                .ToArray();
        }
        catch
        {
            return Array.Empty<NetworkInterface>();
        }
    }

    public static string GetMacAddress()
    {
        var interfaces = GetActiveNetworkInterfaces();
        foreach (var iface in interfaces)
        {
            if (iface.NetworkInterfaceType != NetworkInterfaceType.Loopback)
            {
                var bytes = iface.GetPhysicalAddress().GetAddressBytes();
                return BitConverter.ToString(bytes).Replace("-", ":");
            }
        }

        return string.Empty;
    }

    public static string[] GetMacAddresses()
    {
        var interfaces = GetActiveNetworkInterfaces();
        return interfaces
            .Where(i => i.NetworkInterfaceType != NetworkInterfaceType.Loopback)
            .Select(i => BitConverter.ToString(i.GetPhysicalAddress().GetAddressBytes()).Replace("-", ":"))
            .ToArray();
    }

    public static bool IsValidIpAddress(string ipAddress)
    {
        return IPAddress.TryParse(ipAddress, out _);
    }

    public static bool IsValidIpv4Address(string ipAddress)
    {
        if (!IPAddress.TryParse(ipAddress, out var ip))
            return false;

        return ip.AddressFamily == AddressFamily.InterNetwork;
    }

    public static bool IsValidIpv6Address(string ipAddress)
    {
        if (!IPAddress.TryParse(ipAddress, out var ip))
            return false;

        return ip.AddressFamily == AddressFamily.InterNetworkV6;
    }

    public static bool IsValidPort(int port)
    {
        return port >= 1 && port <= 65535;
    }

    public static async Task<string> ResolveHostNameAsync(string hostName)
    {
        try
        {
            var entry = await Dns.GetHostEntryAsync(hostName);
            foreach (var ip in entry.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                    return ip.ToString();
            }

            return entry.AddressList.FirstOrDefault()?.ToString() ?? hostName;
        }
        catch
        {
            return hostName;
        }
    }

    public static string ResolveHostName(string hostName)
    {
        return ResolveHostNameAsync(hostName).GetAwaiter().GetResult();
    }

    public static async Task<string[]> ResolveHostNamesAsync(string hostName)
    {
        try
        {
            var entry = await Dns.GetHostEntryAsync(hostName);
            return entry.AddressList.Select(ip => ip.ToString()).ToArray();
        }
        catch
        {
            return new[] { hostName };
        }
    }

    public static string[] ResolveHostNames(string hostName)
    {
        return ResolveHostNamesAsync(hostName).GetAwaiter().GetResult();
    }

    public static string EncodeUrl(string url)
    {
        return Uri.EscapeDataString(url);
    }

    public static string DecodeUrl(string url)
    {
        return Uri.UnescapeDataString(url);
    }

    public static string EncodeUrlComponent(string component)
    {
        return Uri.EscapeDataString(component);
    }

    public static string DecodeUrlComponent(string component)
    {
        return Uri.UnescapeDataString(component);
    }

    public static string BuildQueryString(Dictionary<string, string?> parameters)
    {
        if (parameters == null || parameters.Count == 0)
            return string.Empty;

        var sb = new StringBuilder();
        sb.Append('?');

        foreach (var (key, value) in parameters)
        {
            if (sb.Length > 1)
                sb.Append('&');

            sb.Append(EncodeUrlComponent(key));
            if (value != null)
            {
                sb.Append('=');
                sb.Append(EncodeUrlComponent(value));
            }
        }

        return sb.ToString();
    }

    public static Dictionary<string, string> ParseQueryString(string queryString)
    {
        var result = new Dictionary<string, string>();

        if (string.IsNullOrEmpty(queryString))
            return result;

        if (queryString.StartsWith('?'))
            queryString = queryString.Substring(1);

        foreach (var pair in queryString.Split('&'))
        {
            var parts = pair.Split('=');
            if (parts.Length >= 1)
            {
                var key = DecodeUrlComponent(parts[0]);
                var value = parts.Length > 1 ? DecodeUrlComponent(parts[1]) : string.Empty;
                result[key] = value;
            }
        }

        return result;
    }

    public static async Task<string> DownloadStringAsync(string url, int timeout = 30000)
    {
        using var httpClient = new HttpClient { Timeout = TimeSpan.FromMilliseconds(timeout) };
        return await httpClient.GetStringAsync(url);
    }

    public static string DownloadString(string url, int timeout = 30000)
    {
        return DownloadStringAsync(url, timeout).GetAwaiter().GetResult();
    }

    public static async Task<byte[]> DownloadDataAsync(string url, int timeout = 30000)
    {
        using var httpClient = new HttpClient { Timeout = TimeSpan.FromMilliseconds(timeout) };
        return await httpClient.GetByteArrayAsync(url);
    }

    public static byte[] DownloadData(string url, int timeout = 30000)
    {
        return DownloadDataAsync(url, timeout).GetAwaiter().GetResult();
    }

    public static async Task<bool> UploadStringAsync(string url, string data, string contentType = "application/json", int timeout = 30000)
    {
        try
        {
            using var httpClient = new HttpClient { Timeout = TimeSpan.FromMilliseconds(timeout) };
            var response = await httpClient.PostAsync(url, new StringContent(data, Encoding.UTF8, contentType));
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public static bool UploadString(string url, string data, string contentType = "application/json", int timeout = 30000)
    {
        return UploadStringAsync(url, data, contentType, timeout).GetAwaiter().GetResult();
    }
}