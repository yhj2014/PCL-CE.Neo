using System;
using System.Net;

namespace PCL_CE.Neo.Core.Link.Broadcast;

public record BroadcastRecord(string Description, IPEndPoint Address, DateTime FoundAt);
