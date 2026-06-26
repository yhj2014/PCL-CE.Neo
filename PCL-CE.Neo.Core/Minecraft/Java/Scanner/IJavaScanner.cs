using System.Collections.Generic;

namespace PCL_CE.Neo.Core.Minecraft.Java;

/// <summary>
/// Interface for scanning Java installations on the system.
/// </summary>
public interface IJavaScanner
{
    /// <summary>
    /// Scan for Java installations on the system.
    /// </summary>
    /// <param name="results">Collection to add discovered Java paths.</param>
    void Scan(ICollection<string> results);

    /// <summary>
    /// Get scanner name for logging purposes.
    /// </summary>
    string Name { get; }
}