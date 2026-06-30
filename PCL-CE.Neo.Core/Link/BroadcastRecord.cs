using System;
using System.Net;

namespace PCL_CE.Neo.Core.Link;

public record BroadcastRecord(string Desc, IPEndPoint Address, DateTime FoundAt);