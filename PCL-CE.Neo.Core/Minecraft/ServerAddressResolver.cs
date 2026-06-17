using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace PCL_CE.Neo.Core.Minecraft;

public static class ServerAddressResolver
{
    public readonly record struct ResolvedServerAddress(string Host, string? Ip, int Port);

    private const int DefaultPort = 25565;
    private static readonly TimeSpan _HappyEyeballsStagger = TimeSpan.FromMilliseconds(250);
    private static readonly TimeSpan _ConnectTimeout = TimeSpan.FromSeconds(2.5);

    private static readonly Regex _BracketedIpv6 =
        new(@"^\[(?<ip>.+?)\](?::(?<port>\d{1,5}))?$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex _TrailingPort =
        new(@":(?<port>\d{1,5})$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public static async Task<ResolvedServerAddress> GetResolvedServerAddressAsync(string address, CancellationToken cancelToken = default)
    {
        if (string.IsNullOrWhiteSpace(address))
            throw new ArgumentException("服务器地址不能为空", nameof(address));

        address = _NormalizeInput(address);

        var (hostOrIp, portOpt) = _ParseHostAndPort(address);

        if (portOpt is { } explicitPort)
        {
            _ValidatePort(explicitPort);
            var target = await _ResolveReachableAsync(hostOrIp, explicitPort, cancelToken).ConfigureAwait(false);
            if (target is not null)
                return new ResolvedServerAddress(hostOrIp, target.Value.Ip, target.Value.Port);

            var fallbackIp = await _ResolveFirstIpAsync(hostOrIp, cancelToken).ConfigureAwait(false);
            return new ResolvedServerAddress(hostOrIp, fallbackIp, explicitPort);
        }

        if (IPAddress.TryParse(hostOrIp, out _))
        {
            var target = await _ResolveReachableAsync(hostOrIp, DefaultPort, cancelToken).ConfigureAwait(false);
            if (target is not null)
                return new ResolvedServerAddress(hostOrIp, target.Value.Ip, target.Value.Port);

            return new ResolvedServerAddress(hostOrIp, hostOrIp, DefaultPort);
        }

        var idnHost = _ToAsciiIdn(hostOrIp);

        var ip = await _ResolveFirstIpAsync(idnHost, cancelToken).ConfigureAwait(false);
        return new ResolvedServerAddress(idnHost, ip, DefaultPort);
    }

    private static string _NormalizeInput(string input)
    {
        var s = input.Trim();

        var schemeIdx = s.IndexOf("://", StringComparison.Ordinal);
        if (schemeIdx > 0)
            s = s[(schemeIdx + 3)..];

        while (s.EndsWith("/", StringComparison.Ordinal))
            s = s[..^1];

        return s;
    }

    private static void _ValidatePort(int port)
    {
        if (port is < 1 or > 65535)
            throw new FormatException($"无效的端口：{port}");
    }

    private static string _ToAsciiIdn(string host)
    {
        try
        {
            var idn = new IdnMapping();
            var h = _TrimTrailingDot(host);
            return idn.GetAscii(h) + (host.EndsWith(".", StringComparison.Ordinal) ? "." : "");
        }
        catch
        {
            return host;
        }
    }

    private static string _TrimTrailingDot(string host)
        => host.EndsWith(".", StringComparison.Ordinal) ? host[..^1] : host;

    private static (string HostOrIp, int? Port) _ParseHostAndPort(string input)
    {
        var m = _BracketedIpv6.Match(input);
        if (m.Success)
        {
            var ip = m.Groups["ip"].Value;
            if (!IPAddress.TryParse(ip, out _))
                throw new FormatException("无效的 IPv6 地址格式");
            var portGroup = m.Groups["port"];
            if (portGroup.Success)
            {
                var port = int.Parse(portGroup.Value, CultureInfo.InvariantCulture);
                _ValidatePort(port);
                return (ip, port);
            }
            return (ip, null);
        }

        if (IPAddress.TryParse(input, out _))
            return (input, null);

        var pm = _TrailingPort.Match(input);
        if (pm.Success)
        {
            var colonCount = input.Count(c => c == ':');
            if (colonCount == 1)
            {
                var port = int.Parse(pm.Groups["port"].Value, CultureInfo.InvariantCulture);
                _ValidatePort(port);
                var host = input[..^pm.Value.Length];
                if (string.IsNullOrWhiteSpace(host))
                    throw new FormatException("无效的主机名");
                return (host, port);
            }
        }

        return (input, null);
    }

    private static async Task<(string Ip, int Port)?> _ResolveReachableAsync(string hostOrIp, int port, CancellationToken ct)
    {
        try
        {
            if (IPAddress.TryParse(hostOrIp, out var ipLiteral))
            {
                var result = await _ConnectOneAsync(ipLiteral, port, ct).ConfigureAwait(false);
                if (result.ok) return (ipLiteral.ToString(), port);
                return null;
            }

            var addresses = await Dns.GetHostAddressesAsync(_TrimTrailingDot(hostOrIp), ct).ConfigureAwait(false);
            if (addresses.Length == 0)
                return null;

            var v6 = addresses.Where(a => a.AddressFamily == AddressFamily.InterNetworkV6).ToArray();
            var v4 = addresses.Where(a => a.AddressFamily == AddressFamily.InterNetwork).ToArray();

            var winner = await _ConnectAnyAsync(v6, port, TimeSpan.Zero, ct).ConfigureAwait(false);
            if (winner is not null)
                return (winner, port);

            winner = await _ConnectAnyAsync(v4, port, _HappyEyeballsStagger, ct).ConfigureAwait(false);
            if (winner is not null)
                return (winner, port);

            return null;
        }
        catch (Exception) when (ct.IsCancellationRequested == false)
        {
            return null;
        }
    }

    private static async Task<string?> _ResolveFirstIpAsync(string hostOrIp, CancellationToken ct)
    {
        try
        {
            if (IPAddress.TryParse(hostOrIp, out var ip))
                return ip.ToString();

            var addresses = await Dns.GetHostAddressesAsync(_TrimTrailingDot(hostOrIp), ct).ConfigureAwait(false);
            var chosen = addresses.FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetworkV6)
                         ?? addresses.FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork);
            return chosen?.ToString();
        }
        catch (Exception) when (ct.IsCancellationRequested == false)
        {
            return null;
        }
    }

    private static async Task<string?> _ConnectAnyAsync(IReadOnlyList<IPAddress> addrs, int port, TimeSpan delay, CancellationToken ct)
    {
        if (addrs.Count == 0) return null;
        if (delay > TimeSpan.Zero)
            await Task.Delay(delay, ct).ConfigureAwait(false);

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        var tasks = new List<Task<(bool ok, string ip)>>(addrs.Count);
        tasks.AddRange(addrs.Select(ip => _ConnectOneAsync(ip, port, cts.Token)));

        while (tasks.Count > 0)
        {
            var done = await Task.WhenAny(tasks).ConfigureAwait(false);
            tasks.Remove(done);
            var (ok, ip) = await done.ConfigureAwait(false);
            if (!ok) continue;
            try { await cts.CancelAsync().ConfigureAwait(false); } catch { }
            return ip;
        }

        return null;
    }

    private static async Task<(bool ok, string ip)> _ConnectOneAsync(IPAddress ip, int port, CancellationToken ct)
    {
        try
        {
            using var sock = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            sock.NoDelay = true;

            using var timeoutCts = new CancellationTokenSource(_ConnectTimeout);
            using var linked = CancellationTokenSource.CreateLinkedTokenSource(ct, timeoutCts.Token);
            await sock.ConnectAsync(new IPEndPoint(ip, port), linked.Token).ConfigureAwait(false);
            return (true, ip.ToString());
        }
        catch
        {
            return (false, ip.ToString());
        }
    }
}