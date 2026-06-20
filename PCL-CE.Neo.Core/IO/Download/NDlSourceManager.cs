using System.Collections.Generic;

namespace PCL_CE.Neo.Core.IO.Download;

public class NDlSourceManager<TSourceArgument>(IList<IDlResourceMapping<TSourceArgument>> sources)
    : IDlResourceMapping<TSourceArgument>
{
    public IList<IDlResourceMapping<TSourceArgument>> Sources { get; } = sources;

    public TSourceArgument? Parse(string resId)
    {
        foreach (var source in Sources)
        {
            var result = source.Parse(resId);
            if (result != null) return result;
        }
        return default;
    }
}