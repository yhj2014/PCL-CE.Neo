using System.Collections.Generic;

namespace PCL_CE.Neo.Core.Link.Scaffolding.EasyTier;

public record EtPlayerList(IReadOnlyList<EasyPlayerInfo>? Players, EasyPlayerInfo? Host);