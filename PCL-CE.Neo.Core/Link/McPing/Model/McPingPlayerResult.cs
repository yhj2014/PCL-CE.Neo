using System.Collections.Generic;

namespace PCL_CE.Neo.Core.Link.McPing.Model;

public record McPingPlayerResult(int Online, int Max, List<McPingPlayerSampleResult>? Sample);