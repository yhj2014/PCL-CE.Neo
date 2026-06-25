using System.Collections.Generic;

namespace PCL_CE.Neo.Core.Link.McPing.Model;

public record McPingPlayerResult(
    int Max,
    int Online,
    IReadOnlyList<McPingPlayerSampleResult>? Sample);
