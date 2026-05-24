using System.Text.Json;
using Microsoft.Extensions.Logging;
using PCL_CE.Neo.Core.Abstractions;

namespace PCL_CE.Neo.Core.Adapters;

public class ConfigAdapter : IConfigAdapter
{
    private readonly ILogger<ConfigAdapter> _logger;
    private readonly IPathsAdapter _pathsAdapter;
    private readonly Dictionary<string, object?> _config = new();
    private readonly Dictionary<string, Dictionary<string, object?>> _instanceConfig = new();
    private readonly string _configFilePath;
    private readonly SemaphoreSlim _lock = new(1, 1);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public ConfigAdapter()
    {
        _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<ConfigAdapter>.Instance;
        _pathsAdapter = new PathsAdapter();
        _configFilePath = Path.Combine(_pathsAdapter.SharedData, "config.json");
    }

    public ConfigAdapter(ILogger<ConfigAdapter> logger, IPathsAdapter pathsAdapter)
    {
        _logger = logger;
        _pathsAdapter = pathsAdapter;
        _configFilePath = Path.Combine(pathsAdapter.SharedData, "config.json");
    }

    public T GetConfig<T>(string key, T defaultValue = default!)
    {
        lock (_config)
        {
            if (_config.TryGetValue(key, out var value) && value != null)
            {
                try
                {
                    if (value is JsonElement element)
                    {
                        return JsonSerializer.Deserialize<T>(element.GetRawText(), JsonOptions) ?? defaultValue;
                    }
                    if (value is T typedValue)
                    {
                        return typedValue;
                    }
                    return JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(value), JsonOptions) ?? defaultValue;
                }
                catch
                {
                    return defaultValue;
                }
            }
            return defaultValue;
        }
    }

    public void SetConfig<T>(string key, T value)
    {
        lock (_config)
        {
            _config[key] = value;
        }
        _ = SaveConfigAsync();
    }

    public T GetInstanceConfig<T>(string instanceId, string key, T defaultValue = default!)
    {
        lock (_instanceConfig)
        {
            if (_instanceConfig.TryGetValue(instanceId, out var instance) &&
                instance.TryGetValue(key, out var value) && value != null)
            {
                try
                {
                    if (value is T typedValue)
                    {
                        return typedValue;
                    }
                    return JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(value), JsonOptions) ?? defaultValue;
                }
                catch
                {
                    return defaultValue;
                }
            }
            return defaultValue;
        }
    }

    public void SetInstanceConfig<T>(string instanceId, string key, T value)
    {
        lock (_instanceConfig)
        {
            if (!_instanceConfig.ContainsKey(instanceId))
            {
                _instanceConfig[instanceId] = new Dictionary<string, object?>();
            }
            _instanceConfig[instanceId][key] = value;
        }
        _ = SaveInstanceConfigAsync(instanceId);
    }

    public bool HasConfig(string key)
    {
        lock (_config)
        {
            return _config.ContainsKey(key);
        }
    }

    public async Task LoadConfigAsync()
    {
        await _lock.WaitAsync();
        try
        {
            if (File.Exists(_configFilePath))
            {
                var json = await File.ReadAllTextAsync(_configFilePath);
                var loaded = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json, JsonOptions);
                if (loaded != null)
                {
                    lock (_config)
                    {
                        foreach (var kvp in loaded)
                        {
                            _config[kvp.Key] = kvp.Value;
                        }
                    }
                    _logger.LogInformation("配置已加载，共 {Count} 项", loaded.Count);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "加载配置失败");
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task SaveConfigAsync()
    {
        await _lock.WaitAsync();
        try
        {
            Dictionary<string, object?> toSave;
            lock (_config)
            {
                toSave = new Dictionary<string, object?>(_config);
            }

            var json = JsonSerializer.Serialize(toSave, JsonOptions);
            var directory = Path.GetDirectoryName(_configFilePath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }
            await File.WriteAllTextAsync(_configFilePath, json);
            _logger.LogDebug("配置已保存");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "保存配置失败");
        }
        finally
        {
            _lock.Release();
        }
    }

    public void LoadConfig() => LoadConfigAsync().GetAwaiter().GetResult();
    public void SaveConfig() => _ = SaveConfigAsync();

    private async Task SaveInstanceConfigAsync(string instanceId)
    {
        try
        {
            var filePath = Path.Combine(_pathsAdapter.Data, "instances", instanceId, "config.json");
            Dictionary<string, object?> toSave;
            lock (_instanceConfig)
            {
                if (!_instanceConfig.TryGetValue(instanceId, out var instance))
                {
                    return;
                }
                toSave = new Dictionary<string, object?>(instance);
            }

            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var json = JsonSerializer.Serialize(toSave, JsonOptions);
            await File.WriteAllTextAsync(filePath, json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "保存实例配置失败: {InstanceId}", instanceId);
        }
    }
}
