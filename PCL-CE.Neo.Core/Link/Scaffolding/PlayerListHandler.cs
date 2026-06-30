using System.Collections.Generic;
using System.Linq;
using PCL_CE.Neo.Core.Link.Scaffolding.Client.Models;

namespace PCL_CE.Neo.Core.Link.Scaffolding;

public class PlayerListHandler
{
    public static List<PlayerProfile> Sort(IReadOnlyList<PlayerProfile> list)
    {
        var sorted = new List<PlayerProfile>();
        foreach (var profile in list)
        {
            if (profile.Kind == PlayerKind.HOST)
            {
                sorted.Insert(0, profile);
            }
            else
            {
                sorted.Add(profile);
            }
        }
        return sorted;
    }
}