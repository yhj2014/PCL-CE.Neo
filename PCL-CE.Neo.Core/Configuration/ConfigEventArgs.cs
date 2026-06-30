using System;

namespace PCL_CE.Neo.Core.Configuration;

public sealed class ConfigEventArgs
{
    public ConfigItem Item { get; }
    public ConfigEvent Event { get; }
    public object? Argument { get; }
    public object? OldValue { get; }
    public object? NewValue { get; }
    public bool Cancelled { get; set; }
    public object? NewValueReplacement { get; set; }

    public ConfigEventArgs(ConfigItem item, ConfigEvent @event, object? argument, object? oldValue, object? newValue)
    {
        Item = item;
        Event = @event;
        Argument = argument;
        OldValue = oldValue;
        NewValue = newValue;
    }
}