using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace PCL_CE.Neo.Core.IO.Net;

public static class NetworkInterfaceUtils
{
    public static List<NetworkInterface> GetAvailableInterface()
    {
        return NetworkInterface.GetAllNetworkInterfaces()
            .Where(iface => !_IsVirtualInterface(iface))
            .ToList();
    }

    public enum IPv6Status
    {
        Unknown,
        Public,
        RFC4193,
        Unavailable,
    }

    public static IPv6Status GetIPv6Status()
    {
        foreach (var iface in GetAvailableInterface())
        {
            var ipv6Addresses = iface.GetIPProperties().UnicastAddresses
                .Where(addr => addr.Address.AddressFamily == AddressFamily.InterNetworkV6)
                .Select(addr => addr.Address)
                .ToArray();

            if (ipv6Addresses.Length == 0)
            {
                return IPv6Status.Unavailable;
            }

            foreach (var ip in ipv6Addresses)
            {
                if (_IsPublicIPv6(ip))
                {
                    return IPv6Status.Public;
                }
                if (_IsUniqueLocalIPv6(ip))
                {
                    return IPv6Status.RFC4193;
                }
            }

        }

        return IPv6Status.Unknown;
    }

    private static bool _IsVirtualInterface(NetworkInterface iface)
    {
        var virtualTypes = new[] {
            NetworkInterfaceType.Loopback,
            NetworkInterfaceType.Tunnel,
            NetworkInterfaceType.Ppp
        };

        var virtualKeywords = new[] {
            "virtual",
            "pseudo",
            "loopback",
            "tunnel",
            "vpn",
            "ppp",
            "veth",
            "docker",
            "hyper-v",
            "vmware",
            "virtualbox"
        };

        return virtualTypes.Contains(iface.NetworkInterfaceType) ||
               virtualKeywords.Any(keyword => iface.Description.ToLower().Contains(keyword));
    }

    private static bool _IsPublicIPv6(IPAddress ip)
    {
        byte[] addressBytes = ip.GetAddressBytes();
        return addressBytes[0] >= 0x20 && addressBytes[0] <= 0x3F;
    }

    private static bool _IsUniqueLocalIPv6(IPAddress ip)
    {
        byte[] addressBytes = ip.GetAddressBytes();
        return addressBytes[0] == 0xFC || addressBytes[0] == 0xFD;
    }
}