using System;
using System.Text.Json.Serialization;

namespace PCL_CE.Neo.Core.Minecraft.Java;

/// <summary>
/// Serialized Java configuration item for storage.
/// </summary>
public sealed class JavaStorageItem
{
    /// <summary>
    /// Path to Java executable.
    /// </summary>
    [JsonPropertyName("path")]
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// Whether this Java is enabled.
    /// </summary>
    [JsonPropertyName("isEnable")]
    public bool IsEnable { get; set; } = true;

    /// <summary>
    /// How this Java was added.
    /// </summary>
    [JsonPropertyName("source")]
    public JavaSource Source { get; set; } = JavaSource.AutoScanned;
}