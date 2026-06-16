namespace PCL_CE.Neo.Core.Configuration;

public class ConfigObserver(ConfigEvent configEvent, ConfigEventHandler handler, bool isPreview = false)
{
    public ConfigEvent Event { get; } = configEvent;
    public ConfigEventHandler Handler { get; } = handler;
    public bool IsPreview { get; } = isPreview;
}