using System;

namespace PCL_CE.Neo.Core.Link;

public class BroadcastRecord
{
    public string ServerAddress { get; set; } = string.Empty;
    public int Port { get; set; } = 25565;
    public string? ServerName { get; set; }
    public int ProtocolVersion { get; set; }
    public string? VersionName { get; set; }
    public int MaxPlayers { get; set; } = 20;
    public int CurrentPlayers { get; set; } = 0;
    public string? Description { get; set; }
    public string? Favicon { get; set; }
    public DateTime LastSeen { get; set; } = DateTime.Now;
    public int Latency { get; set; } = 0;
    public bool IsLocal { get; set; } = false;
}

public class BroadcastOptions
{
    public int BroadcastPort { get; set; } = 4445;
    public int ListenPort { get; set; } = 4445;
    public TimeSpan BroadcastInterval { get; set; } = TimeSpan.FromSeconds(5);
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(10);
    public int MaxRecords { get; set; } = 50;
}