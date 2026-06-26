namespace PCL_CE.Neo.Core.Minecraft.Java;

/// <summary>
/// Interface for parsing Java installation information from executable path.
/// </summary>
public interface IJavaParser
{
    /// <summary>
    /// Parse Java installation details from the given executable path.
    /// </summary>
    /// <param name="javaExePath">Path to java executable (java.exe or java).</param>
    /// <returns>Parsed Java installation information, or null if parsing fails.</returns>
    JavaInstallation? Parse(string javaExePath);
}