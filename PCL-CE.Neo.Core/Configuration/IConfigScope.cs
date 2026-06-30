using System.Collections.Generic;

namespace PCL_CE.Neo.Core.Configuration;

public interface IConfigScope
{
    IEnumerable<string> CheckScope(IReadOnlySet<string> keys);
}