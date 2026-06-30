using System.Collections.Generic;

namespace PCL_CE.Neo.Core.Configuration.Storage;

public interface IEnumerableKeyProvider
{
    IEnumerable<string> Keys { get; }
}