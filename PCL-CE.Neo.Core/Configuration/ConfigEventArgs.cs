using System;

namespace PCL_CE.Neo.Core.Configuration;

public class ConfigEventArgs(
    string key,
    ConfigEvent configEvent,
    object? argument,
    object? oldValue,
    object? newValue)
{
    public string Key { get; } = key;
    public ConfigEvent ConfigEvent { get; } = configEvent;
    public object? Argument { get; } = argument;
    public object? OldValue { get; } = oldValue;
    public object? NewValue { get; } = newValue;
    public bool Cancelled { get; set; }
    public object? NewValueReplacement { get; set; }
}