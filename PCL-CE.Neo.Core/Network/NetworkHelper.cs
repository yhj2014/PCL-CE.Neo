using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using PCL_CE.Neo.Core.Logging;

namespace PCL_CE.Neo.Core.Network;

public static class NetworkHelper
{
    public static bool IsNetworkAvailable()
    {
        try
        {
            return NetworkInterface.GetIsNetworkAvailable();
        }
        catch (Exception ex)
        {
            LogWrapper.Warn(ex, "Failed to check network availability");
            return false;
        }
    }

    public static async Task<bool> IsInternetAvailableAsync(int timeout = 3000, CancellationToken cancellationToken = default)
    {
        try
        {
            var hosts = new[] { "8.8.8.8", "1.1.1.1", "208.67.222.222" };

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(timeout);

            foreach (var host in hosts)
            {
                try
                {
                    using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    socket.NoDelay = true;

                    await socket.ConnectAsync(host, 53, cts.Token);
                    return true;
                }
                catch (OperationCanceledException)
                {
                    return false;
                }
                catch
                {
                    continue;
                }
            }

            return false;
        }
        catch (Exception ex)
        {
            LogWrapper.Warn(ex, "Failed to check internet availability");
            return false;
        }
    }

    public static async Task<bool> TestConnectionAsync(string host, int port, int timeout = 5000, CancellationToken cancellationToken = default)
    {
        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(timeout);

            using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.NoDelay = true;

            await socket.ConnectAsync(host, port, cts.Token);
            return true;
        }
        catch (Exception ex) when (ex is SocketException or OperationCanceledException)
        {
            return false;
        }
        catch (Exception ex)
        {
            LogWrapper.Warn(ex, $"Failed to test connection to {host}:{port}");
            return false;
        }
    }

    public static IPAddress[] GetLocalIpAddresses()
    {
        try
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            return host.AddressList.Where(ip => ip.AddressFamily == AddressFamily.InterNetwork).ToArray();
        }
        catch (Exception ex)
        {
            LogWrapper.Warn(ex, "Failed to get local IP addresses");
            return Array.Empty<IPAddress>();
        }
    }

    public static int GetAvailablePort(int startPort = 1024, int endPort = 65535)
    {
        try
        {
            for (int port = startPort; port <= endPort; port++)
            {
                try
                {
                    using var listener = new TcpListener(IPAddress.Loopback, port);
                    listener.Start();
                    listener.Stop();
                    return port;
                }
                catch
                {
                    continue;
                }
            }
        }
        catch (Exception ex)
        {
            LogWrapper.Warn(ex, "Failed to find available port");
        }

        throw new InvalidOperationException("No available port found in the specified range");
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

    public static bool IsPortInUse(int port)
    {
        return !IsPortAvailable(port);
    }

    public static string GetPublicIpAddress()
    {
        try
        {
            var client = new WebClient();
            return client.DownloadString("https://api.ipify.org").Trim();
        }
        catch (Exception ex)
        {
            LogWrapper.Warn(ex, "Failed to get public IP address");
            return string.Empty;
        }
    }

    public static async Task<string> GetPublicIpAddressAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var client = new HttpClient();
            using var response = await client.GetAsync("https://api.ipify.org", cancellationToken);
            response.EnsureSuccessStatusCode();
            return (await response.Content.ReadAsStringAsync(cancellationToken)).Trim();
        }
        catch (Exception ex)
        {
            LogWrapper.Warn(ex, "Failed to get public IP address");
            return string.Empty;
        }
    }
}