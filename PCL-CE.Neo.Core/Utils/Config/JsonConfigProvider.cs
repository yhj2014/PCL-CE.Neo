using System.Text.Json;
using PCL_CE.Neo.Core.Utils.FileSystem;

namespace PCL_CE.Neo.Core.Utils.Config;

public class JsonConfigProvider : IConfigProvider
{
    private readonly string _configPath;
    private readonly JsonSerializerOptions _serializerOptions;
    private Dictionary<string, object> _config = new Dictionary<string, object>();

    public JsonConfigProvider(string configPath)
    {
        _configPath = configPath;
        _serializerOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true,
            AllowTrailingCommas = true
        };
        Load();
    }

    public T Get<T>(string key, T defaultValue = default!)
    {
        if (_config.TryGetValue(key, out var value))
        {
            try
            {
                if (value is T typedValue)
                    return typedValue;

                string json = JsonSerializer.Serialize(value);
                return JsonSerializer.Deserialize<T>(json, _serializerOptions) ?? defaultValue;
            }
            catch
            {
                return defaultValue;
            }
        }
        return defaultValue;
    }

    public bool TryGet<T>(string key, out T value)
    {
        value = default!;
        if (!_config.TryGetValue(key, out var rawValue))
            return false;

        try
        {
            if (rawValue is T typedValue)
            {
                value = typedValue;
                return true;
            }

            string json = JsonSerializer.Serialize(rawValue);
            value = JsonSerializer.Deserialize<T>(json, _serializerOptions) ?? default!;
            return value != null || typeof(T).IsValueType;
        }
        catch
        {
            return false;
        }
    }

    public void Set<T>(string key, T value)
    {
        _config[key] = value ?? throw new ArgumentNullException(nameof(value));
    }

    public bool ContainsKey(string key)
    {
        return _config.ContainsKey(key);
    }

    public void Remove(string key)
    {
        _config.Remove(key);
    }

    public void Clear()
    {
        _config.Clear();
    }

    public void Save()
    {
        FileUtils.EnsureParentDirectoryExists(_configPath);
        string json = JsonSerializer.Serialize(_config, _serializerOptions);
        FileUtils.WriteAllText(_configPath, json);
    }

    public void Load()
    {
        if (!FileUtils.Exists(_configPath))
        {
            _config = new Dictionary<string, object>();
            return;
        }

        try
        {
            string json = FileUtils.ReadAllText(_configPath);
            _config = JsonSerializer.Deserialize<Dictionary<string, object>>(json, _serializerOptions)
                ?? new Dictionary<string, object>();
        }
        catch
        {
            _config = new Dictionary<string, object>();
        }
    }

    public IDictionary<string, object> GetAll()
    {
        return new Dictionary<string, object>(_config);
    }
}