using System.Net;

namespace PCL_CE.Neo.Core.Link;

public record BroadcastRecord(
    string Motd,
    IPEndPoint Endpoint,
    DateTime ReceivedAt);