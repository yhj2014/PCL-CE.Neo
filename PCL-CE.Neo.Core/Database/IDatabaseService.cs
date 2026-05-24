using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PCL_CE.Neo.Core.Abstractions;
using PCL_CE.Neo.Core.App;

namespace PCL_CE.Neo.Core.Database;

public interface IDatabaseService
{
    T? Get<T>(string key) where T : class;
    void Set<T>(string key, T value) where T : class;
    bool Delete(string key);
    bool Exists(string key);
    IEnumerable<string> GetKeys(string? prefix = null);
    void Clear();
}

public class DatabaseService : IDatabaseService, IDisposable
{
    private readonly ILogger<DatabaseService> _logger;
    private readonly string _databasePath;
    private readonly Dictionary<string, object> _cache = new();
    private readonly SemaphoreSlim _lock = new(1, 1);

    public DatabaseService() : this(
        Microsoft.Extensions.Logging.Abstractions.NullLogger<DatabaseService>.Instance,
        Path.Combine(Paths.SharedData, "database.json"))
    {
    }

    public DatabaseService(ILogger<DatabaseService> logger, string databasePath)
    {
        _logger = logger;
        _databasePath = databasePath;
        Load();
    }

    private void Load()
    {
        try
        {
            if (File.Exists(_databasePath))
            {
                var json = File.ReadAllText(_databasePath);
                var data = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(json);
                if (data != null)
                {
                    lock (_cache)
                    {
                        foreach (var kvp in data)
                        {
                            _cache[kvp.Key] = kvp.Value;
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load database");
        }
    }

    private void Save()
    {
        try
        {
            Dictionary<string, object> toSave;
            lock (_cache)
            {
                toSave = new Dictionary<string, object>(_cache);
            }
            var json = System.Text.Json.JsonSerializer.Serialize(toSave, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
            var directory = Path.GetDirectoryName(_databasePath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }
            File.WriteAllText(_databasePath, json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save database");
        }
    }

    public T? Get<T>(string key) where T : class
    {
        lock (_cache)
        {
            if (_cache.TryGetValue(key, out var value))
            {
                if (value is T typed)
                    return typed;
                try
                {
                    var json = System.Text.Json.JsonSerializer.Serialize(value);
                    return System.Text.Json.JsonSerializer.Deserialize<T>(json);
                }
                catch
                {
                    return null;
                }
            }
            return null;
        }
    }

    public void Set<T>(string key, T value) where T : class
    {
        lock (_cache)
        {
            _cache[key] = value;
        }
        Save();
    }

    public bool Delete(string key)
    {
        lock (_cache)
        {
            var result = _cache.Remove(key);
            if (result) Save();
            return result;
        }
    }

    public bool Exists(string key)
    {
        lock (_cache)
        {
            return _cache.ContainsKey(key);
        }
    }

    public IEnumerable<string> GetKeys(string? prefix = null)
    {
        lock (_cache)
        {
            if (string.IsNullOrEmpty(prefix))
                return _cache.Keys.ToList();
            return _cache.Keys.Where(k => k.StartsWith(prefix)).ToList();
        }
    }

    public void Clear()
    {
        lock (_cache)
        {
            _cache.Clear();
        }
        Save();
    }

    private bool _disposed;
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _lock.Dispose();
    }
}

public static class DatabaseExtensions
{
    public static IServiceCollection AddDatabaseService(this IServiceCollection services)
    {
        services.AddSingleton<IDatabaseService, DatabaseService>();
        return services;
    }
}
