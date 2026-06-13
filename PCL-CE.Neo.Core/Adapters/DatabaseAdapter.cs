using Microsoft.Extensions.Logging;
using PCL_CE.Neo.Core.Abstractions;

namespace PCL_CE.Neo.Core.Adapters;

public class DatabaseAdapter : IDatabaseAdapter
{
    private readonly ILogger<DatabaseAdapter> _logger;
    private readonly IPathsAdapter _pathsAdapter;
    private readonly string _databasePath;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private readonly Dictionary<string, Dictionary<string, string>> _collections = new();

    public DatabaseAdapter() : this(
        Microsoft.Extensions.Logging.Abstractions.NullLogger<DatabaseAdapter>.Instance,
        new PathsAdapter())
    {
    }

    public DatabaseAdapter(ILogger<DatabaseAdapter> logger, IPathsAdapter pathsAdapter)
    {
        _logger = logger;
        _pathsAdapter = pathsAdapter;
        _databasePath = Path.Combine(pathsAdapter.SharedData, "database.json");
        LoadDatabase();
    }

    public void InitializeDatabase()
    {
        // 已在构造函数中初始化
    }

    public string GetConnection()
    {
        return _databasePath;
    }

    public async Task<T?> GetAsync<T>(string collection, string id) where T : class
    {
        await _lock.WaitAsync();
        try
        {
            if (_collections.TryGetValue(collection, out var coll) &&
                coll.TryGetValue(id, out var json))
            {
                return System.Text.Json.JsonSerializer.Deserialize<T>(json);
            }
            return null;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<IEnumerable<T>> GetAllAsync<T>(string collection) where T : class
    {
        await _lock.WaitAsync();
        try
        {
            var results = new List<T>();
            if (_collections.TryGetValue(collection, out var coll))
            {
                foreach (var json in coll.Values)
                {
                    var item = System.Text.Json.JsonSerializer.Deserialize<T>(json);
                    if (item != null) results.Add(item);
                }
            }
            return results;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<bool> InsertAsync<T>(string collection, string id, T item) where T : class
    {
        await _lock.WaitAsync();
        try
        {
            if (!_collections.ContainsKey(collection))
            {
                _collections[collection] = new Dictionary<string, string>();
            }

            if (_collections[collection].ContainsKey(id))
            {
                _logger.LogWarning("尝试插入已存在的文档: {Collection}/{Id}", collection, id);
                return false;
            }

            var json = System.Text.Json.JsonSerializer.Serialize(item);
            _collections[collection][id] = json;
            await SaveDatabaseAsync();
            return true;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<bool> UpdateAsync<T>(string collection, string id, T item) where T : class
    {
        await _lock.WaitAsync();
        try
        {
            if (!_collections.TryGetValue(collection, out var coll) ||
                !coll.ContainsKey(id))
            {
                _logger.LogWarning("尝试更新不存在的文档: {Collection}/{Id}", collection, id);
                return false;
            }

            var json = System.Text.Json.JsonSerializer.Serialize(item);
            coll[id] = json;
            await SaveDatabaseAsync();
            return true;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<bool> DeleteAsync(string collection, string id)
    {
        await _lock.WaitAsync();
        try
        {
            if (!_collections.TryGetValue(collection, out var coll) ||
                !coll.Remove(id))
            {
                return false;
            }

            await SaveDatabaseAsync();
            return true;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<long> CountAsync(string collection)
    {
        await _lock.WaitAsync();
        try
        {
            if (_collections.TryGetValue(collection, out var coll))
            {
                return coll.Count;
            }
            return 0;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<IEnumerable<T>> QueryAsync<T>(string collection, Func<T, bool> predicate) where T : class
    {
        var all = await GetAllAsync<T>(collection);
        return all.Where(predicate);
    }

    private void LoadDatabase()
    {
        try
        {
            if (File.Exists(_databasePath))
            {
                var json = File.ReadAllText(_databasePath);
                var loaded = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(json);
                if (loaded != null)
                {
                    _collections.Clear();
                    foreach (var kvp in loaded)
                    {
                        _collections[kvp.Key] = kvp.Value;
                    }
                    _logger.LogInformation("数据库已加载: {Collections} 个集合", _collections.Count);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "数据库加载失败");
        }
    }

    private async Task SaveDatabaseAsync()
    {
        try
        {
            var json = System.Text.Json.JsonSerializer.Serialize(_collections, new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true
            });

            var directory = Path.GetDirectoryName(_databasePath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            await File.WriteAllTextAsync(_databasePath, json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "数据库保存失败");
        }
    }
}
