using Microsoft.Extensions.Logging;
using PCL_CE.Neo.Core.Abstractions;

namespace PCL_CE.Neo.Core.Adapters;

public class StateAdapter : IStateAdapter
{
    private readonly ILogger<StateAdapter> _logger;
    private readonly IConfigAdapter _config;
    private readonly Dictionary<string, object> _state = new();

    public StateAdapter() : this(
        Microsoft.Extensions.Logging.Abstractions.NullLogger<StateAdapter>.Instance,
        new ConfigAdapter())
    {
    }

    public StateAdapter(ILogger<StateAdapter> logger, IConfigAdapter config)
    {
        _logger = logger;
        _config = config;
    }

    public Dictionary<string, object> State => _state;

    public T? GetState<T>(string key, T? defaultValue = default)
    {
        return _state.TryGetValue(key, out var value) ? (T)value : defaultValue;
    }

    public void SetState<T>(string key, T value)
    {
        _state[key] = value!;
    }

    public void ClearState(string key)
    {
        _state.Remove(key);
    }

    public string Identifier
    {
        get => _config.GetConfig("Identify", "");
        set => _config.SetConfig("Identify", value);
    }

    public Dictionary<string, string> CustomVariables
    {
        get => _config.GetConfig("CustomVariables", new Dictionary<string, string>());
        set => _config.SetConfig("CustomVariables", value);
    }

    public double WindowWidth
    {
        get => _config.GetConfig("WindowWidth", 900.0);
        set => _config.SetConfig("WindowWidth", value);
    }

    public double WindowHeight
    {
        get => _config.GetConfig("WindowHeight", 550.0);
        set => _config.SetConfig("WindowHeight", value);
    }

    public string SelectedInstance
    {
        get => _config.GetConfig("LaunchInstanceSelect", "");
        set => _config.SetConfig("LaunchInstanceSelect", value);
    }

    public string SelectedFolder
    {
        get => _config.GetConfig("LaunchFolderSelect", "");
        set => _config.SetConfig("LaunchFolderSelect", value);
    }

    public string Folders
    {
        get => _config.GetConfig("LaunchFolders", "");
        set => _config.SetConfig("LaunchFolders", value);
    }

    public string JavaList
    {
        get => _config.GetConfig("LaunchArgumentJavaUser", "[]");
        set => _config.SetConfig("LaunchArgumentJavaUser", value);
    }

    public int StartupCount
    {
        get => _config.GetConfig("SystemCount", 0);
        set => _config.SetConfig("SystemCount", value);
    }

    public int LaunchCount
    {
        get => _config.GetConfig("SystemLaunchCount", 0);
        set => _config.SetConfig("SystemLaunchCount", value);
    }

    public string DownloadFolder
    {
        get => _config.GetConfig("CacheDownloadFolder", "");
        set => _config.SetConfig("CacheDownloadFolder", value);
    }
}
