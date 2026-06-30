namespace PCL_CE.Neo.Core.Configuration;

public sealed class ConfigObserver
{
    public ConfigEvent Event { get; }
    public ConfigEventHandler Handler { get; }
    public bool IsPreview { get; }

    public ConfigObserver(ConfigEvent @event, ConfigEventHandler handler, bool isPreview = false)
    {
        Event = @event;
        Handler = handler;
        IsPreview = isPreview;
    }
}