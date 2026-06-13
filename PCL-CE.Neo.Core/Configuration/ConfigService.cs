using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PCL_CE.Neo.Core.Abstractions;
using PCL_CE.Neo.Core.Adapters;
using PCL_CE.Neo.Core.App;

namespace PCL_CE.Neo.Core.Configuration;

public interface IConfigService
{
    T GetValue<T>(string key, T defaultValue = default!);
    void SetValue<T>(string key, T value);
    bool HasKey(string key);
    Task LoadAsync();
    Task SaveAsync();
}

public class ConfigService : IConfigService, IDisposable
{
    private readonly ILogger<ConfigService> _logger;
    private readonly IPathsAdapter _pathsAdapter;
    private readonly Dictionary<string, object?> _config = new();
    private readonly SemaphoreSlim _lock = new(1, 1);
    private readonly string _configFilePath;
    private bool _loaded;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public ConfigService() : this(
        Microsoft.Extensions.Logging.Abstractions.NullLogger<ConfigService>.Instance,
        new PathsAdapter())
    {
    }

    public ConfigService(ILogger<ConfigService> logger, string configFilePath)
    {
        _logger = logger;
        _pathsAdapter = new PathsAdapter();
        _configFilePath = configFilePath;
    }

    public ConfigService(ILogger<ConfigService> logger, IPathsAdapter pathsAdapter)
    {
        _logger = logger;
        _pathsAdapter = pathsAdapter;
        _configFilePath = Path.Combine(_pathsAdapter.SharedData, "config.v1.json");
    }

    public T GetValue<T>(string key, T defaultValue = default!)
    {
        lock (_config)
        {
            if (_config.TryGetValue(key, out var value) && value != null)
            {
                try
                {
                    if (value is JsonElement element)
                    {
                        var result = JsonSerializer.Deserialize<T>(element.GetRawText(), JsonOptions);
                        return result ?? defaultValue;
                    }
                    if (value is T typedValue)
                    {
                        return typedValue;
                    }
                    var json = JsonSerializer.Serialize(value, JsonOptions);
                    return JsonSerializer.Deserialize<T>(json, JsonOptions) ?? defaultValue;
                }
                catch
                {
                    return defaultValue;
                }
            }
            return defaultValue;
        }
    }

    public async Task InitializeAsync()
    {
        await LoadAsync();
    }

    public void Set<T>(string key, T value) => SetValue(key, value);

    public T? Get<T>(string key, T? defaultValue = default) => GetValue(key, defaultValue);

    public void SetValue<T>(string key, T value)
    {
        lock (_config)
        {
            _config[key] = value;
        }
        _ = SaveAsync();
    }

    public bool HasKey(string key)
    {
        lock (_config)
        {
            return _config.ContainsKey(key);
        }
    }

    public async Task LoadAsync()
    {
        if (_loaded) return;

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
            _loaded = true;
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

    public async Task SaveAsync()
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

    public void Load() => LoadAsync().GetAwaiter().GetResult();
    public void Save() => _ = SaveAsync();

    private bool _disposed;
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _lock.Dispose();
    }
}

public static class ConfigServiceExtensions
{
    public static IServiceCollection AddConfigService(this IServiceCollection services)
    {
        services.AddSingleton<IConfigService, ConfigService>();
        return services;
    }

    public static IConfigService GetConfigService(this IServiceProvider services)
    {
        return services.GetRequiredService<IConfigService>();
    }
}
