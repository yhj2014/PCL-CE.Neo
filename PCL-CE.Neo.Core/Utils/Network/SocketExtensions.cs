using System.Net;
using System.Net.Sockets;

namespace PCL_CE.Neo.Core.Utils.Network;

public static class SocketExtensions
{
    public static async Task<int> ReceiveAsync(this Socket socket, byte[] buffer, CancellationToken cancellationToken)
    {
        return await socket.ReceiveAsync(buffer, SocketFlags.None, cancellationToken);
    }

    public static async Task<int> ReceiveAsync(this Socket socket, Memory<byte> buffer, CancellationToken cancellationToken)
    {
        return await socket.ReceiveAsync(buffer, SocketFlags.None, cancellationToken);
    }

    public static async Task SendAsync(this Socket socket, byte[] buffer, CancellationToken cancellationToken)
    {
        await socket.SendAsync(buffer, SocketFlags.None, cancellationToken);
    }

    public static async Task SendAsync(this Socket socket, ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken)
    {
        await socket.SendAsync(buffer, SocketFlags.None, cancellationToken);
    }

    public static async Task ConnectAsync(this Socket socket, string host, int port, CancellationToken cancellationToken)
    {
        var addresses = await Dns.GetHostAddressesAsync(host);
        var endpoint = new IPEndPoint(addresses.First(a => a.AddressFamily == socket.AddressFamily), port);
        await socket.ConnectAsync(endpoint, cancellationToken);
    }

    public static async Task<Socket> AcceptAsync(this Socket socket, CancellationToken cancellationToken)
    {
        return await socket.AcceptAsync(cancellationToken);
    }

    public static bool IsConnected(this Socket socket)
    {
        try
        {
            return !(socket.Poll(1, SelectMode.SelectRead) && socket.Available == 0);
        }
        catch
        {
            return false;
        }
    }

    public static void SetKeepAlive(this Socket socket, bool enable, int timeoutMs = 30000, int intervalMs = 10000)
    {
        byte[] keepAliveBytes = new byte[12];
        BitConverter.GetBytes((uint)(enable ? 1 : 0)).CopyTo(keepAliveBytes, 0);
        BitConverter.GetBytes((uint)timeoutMs).CopyTo(keepAliveBytes, 4);
        BitConverter.GetBytes((uint)intervalMs).CopyTo(keepAliveBytes, 8);
        socket.IOControl(IOControlCode.KeepAliveValues, keepAliveBytes, null);
    }

    public static void SetNoDelay(this Socket socket, bool enable)
    {
        socket.NoDelay = enable;
    }

    public static void SetReuseAddress(this Socket socket, bool enable)
    {
        socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, enable);
    }

    public static void SetReceiveBufferSize(this Socket socket, int size)
    {
        socket.ReceiveBufferSize = size;
    }

    public static void SetSendBufferSize(this Socket socket, int size)
    {
        socket.SendBufferSize = size;
    }
}