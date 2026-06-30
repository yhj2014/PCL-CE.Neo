using System;
using PCL_CE.Neo.Core.Link.Scaffolding.Client.Models;

namespace PCL_CE.Neo.Core.Link.Scaffolding.Server;

public record TrackedPlayerProfile
{
    public required PlayerProfile Profile { get; set; }
    public required DateTime LastSeenUtc { get; set; }
}