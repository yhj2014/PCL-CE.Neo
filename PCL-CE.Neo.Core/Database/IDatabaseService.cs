using LiteDB;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PCL_CE.Neo.Core.Logging;
using System.Collections.Concurrent;

namespace PCL_CE.Neo.Core.Database;

public interface IDatabaseService
{
    T? Get<T>(string key) where T : class;
    void Set<T>(string key, T value) where T : class;
    bool Delete(string key);
    bool Exists(string key);
    IEnumerable<string> GetKeys(string? prefix = null);
    void Clear();
    void Dispose();
}

public class DatabaseService : IDatabaseService
{
    private const string ModuleName = "DatabaseService";
    private static readonly ConcurrentDictionary<string, LiteDatabase> _Instances = new();
    private readonly LiteDatabase _database;
    private readonly string _connectionPath;

    public DatabaseService(string connectionPath)
    {
        _connectionPath = connectionPath ?? throw new ArgumentException("Connection string cannot be null or whitespace.", nameof(connectionPath));
        _database = _Instances.GetOrAdd(_connectionPath, cp => new LiteDatabase(cp));
        LogWrapper.Info(ModuleName, $"Database connection opened: {_connectionPath}");
    }

    public T? Get<T>(string key) where T : class
    {
        try
        {
            var collection = _database.GetCollection<T>("data");
            return collection.FindById(key);
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, ModuleName, $"Failed to get document with key: {key}");
            return null;
        }
    }

    public void Set<T>(string key, T value) where T : class
    {
        try
        {
            var collection = _database.GetCollection<T>("data");
            collection.Upsert(key, value);
            LogWrapper.Trace(ModuleName, $"Document set with key: {key}");
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, ModuleName, $"Failed to set document with key: {key}");
        }
    }

    public bool Delete(string key)
    {
        try
        {
            var collection = _database.GetCollection<BsonDocument>("data");
            var result = collection.Delete(key);
            if (result)
                LogWrapper.Trace(ModuleName, $"Document deleted with key: {key}");
            return result;
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, ModuleName, $"Failed to delete document with key: {key}");
            return false;
        }
    }

    public bool Exists(string key)
    {
        try
        {
            var collection = _database.GetCollection<BsonDocument>("data");
            return collection.Exists(key);
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, ModuleName, $"Failed to check existence of key: {key}");
            return false;
        }
    }

    public IEnumerable<string> GetKeys(string? prefix = null)
    {
        try
        {
            var collection = _database.GetCollection<BsonDocument>("data");
            var allKeys = collection.Query().Select(x => x["_id"].AsString).ToList();
            if (string.IsNullOrEmpty(prefix))
                return allKeys;
            return allKeys.Where(k => k.StartsWith(prefix));
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, ModuleName, $"Failed to get keys with prefix: {prefix}");
            return Enumerable.Empty<string>();
        }
    }

    public void Clear()
    {
        try
        {
            _database.DropCollection("data");
            LogWrapper.Info(ModuleName, "Database cleared");
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, ModuleName, "Failed to clear database");
        }
    }

    public void Dispose()
    {
        try
        {
            if (_Instances.TryRemove(_connectionPath, out var instance))
            {
                instance.Dispose();
                LogWrapper.Info(ModuleName, $"Database connection closed: {_connectionPath}");
            }
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, ModuleName, $"Failed to dispose database connection: {_connectionPath}");
        }
    }
}

public static class DatabaseExtensions
{
    public static IServiceCollection AddDatabaseService(this IServiceCollection services)
    {
        services.AddSingleton<IDatabaseService>(sp => 
        {
            var pathsAdapter = sp.GetService<PCL_CE.Neo.Core.Abstractions.IPathsAdapter>();
            var dbPath = pathsAdapter?.GetDatabasePath() ?? "data/database.db";
            return new DatabaseService(dbPath);
        });
        return services;
    }
}