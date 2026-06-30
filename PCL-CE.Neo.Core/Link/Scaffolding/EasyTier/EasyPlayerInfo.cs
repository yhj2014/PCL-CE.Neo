namespace PCL_CE.Neo.Core.Link.Scaffolding.EasyTier;

public class EasyPlayerInfo
{
    public bool IsHost { get; set; }
    public string HostName { get; set; } = string.Empty;
    public string Ip { get; set; } = string.Empty;
    public double Ping { get; set; }
    public double Loss { get; set; }
    public string? NatType { get; set; }
    public string? EasyTierVer { get; set; }
}