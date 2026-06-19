using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace PCL_CE.Neo.Core.Utils.OS;

public static class NetworkUtils
{
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

    public static IPAddress? GetLocalIpAddress()
    {
        try
        {
            using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket.Connect("8.8.8.8", 80);
            var endPoint = socket.LocalEndPoint as IPEndPoint;
            return endPoint?.Address;
        }
        catch
        {
            return null;
        }
    }

    public static IEnumerable<IPAddress> GetAllLocalIpAddresses()
    {
        var addresses = new List<IPAddress>();
        try
        {
            var hostEntry = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var address in hostEntry.AddressList)
            {
                if (address.AddressFamily == AddressFamily.InterNetwork)
                {
                    addresses.Add(address);
                }
            }
        }
        catch
        {
        }
        return addresses;
    }

    public static async Task<bool> PingHostAsync(string host, int timeout = 3000)
    {
        try
        {
            using var ping = new Ping();
            var reply = await ping.SendPingAsync(host, timeout).ConfigureAwait(false);
            return reply.Status == IPStatus.Success;
        }
        catch
        {
            return false;
        }
    }

    public static int FindAvailablePort(int startPort = 10000, int endPort = 65535)
    {
        for (int port = startPort; port <= endPort; port++)
        {
            try
            {
                using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.Bind(new IPEndPoint(IPAddress.Any, port));
                socket.Close();
                return port;
            }
            catch
            {
            }
        }
        throw new IOException("No available port found");
    }
}