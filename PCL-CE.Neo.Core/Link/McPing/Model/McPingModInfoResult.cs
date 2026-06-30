using System.Collections.Generic;

namespace PCL_CE.Neo.Core.Link.McPing.Model;

public record McPingModInfoResult(string Type, List<McPingModInfoModResult> Mods);