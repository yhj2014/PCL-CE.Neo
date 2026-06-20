using System.Net.NetworkInformation;

namespace PCL_CE.Neo.Core.Network;

public static class NetworkInterfaceUtils
{
    public static IPv6Status GetIPv6Status()
    {
        foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (ni.NetworkInterfaceType == NetworkInterfaceType.Loopback)
                continue;
            
            var properties = ni.GetIPProperties();
            if (properties != null)
            {
                var ipv6Properties = properties.GetIPv6Properties();
                if (ipv6Properties != null && ni.OperationalStatus == OperationalStatus.Up)
                {
                    return IPv6Status.Enabled;
                }
            }
        }
        return IPv6Status.Disabled;
    }
}

public enum IPv6Status
{
    Disabled,
    Enabled,
    Unknown
}