using Microsoft.Extensions.Logging;
using PCL_CE.Neo.Core.Database;
using Xunit;

namespace PCL_CE.Neo.Tests;

public class DatabaseServiceTests
{
    [Fact]
    public void DatabaseService_StoresAndRetrievesData()
    {
        var tempPath = Path.Combine(Path.GetTempPath(), $"test_db_{Guid.NewGuid()}.json");
        var service = new DatabaseService(
            Microsoft.Extensions.Logging.Abstractions.NullLogger<DatabaseService>.Instance,
            tempPath);

        service.Set("key1", "value1");
        var result = service.Get<string>("key1");
        
        Assert.Equal("value1", result);
        
        File.Delete(tempPath);
    }

    [Fact]
    public void DatabaseService_ExistsReturnsCorrectValue()
    {
        var tempPath = Path.Combine(Path.GetTempPath(), $"test_db_{Guid.NewGuid()}.json");
        var service = new DatabaseService(
            Microsoft.Extensions.Logging.Abstractions.NullLogger<DatabaseService>.Instance,
            tempPath);

        service.Set("key1", "value1");
        
        Assert.True(service.Exists("key1"));
        Assert.False(service.Exists("nonexistent"));
        
        File.Delete(tempPath);
    }

    [Fact]
    public void DatabaseService_DeleteRemovesEntry()
    {
        var tempPath = Path.Combine(Path.GetTempPath(), $"test_db_{Guid.NewGuid()}.json");
        var service = new DatabaseService(
            Microsoft.Extensions.Logging.Abstractions.NullLogger<DatabaseService>.Instance,
            tempPath);

        service.Set("key1", "value1");
        var deleted = service.Delete("key1");
        
        Assert.True(deleted);
        Assert.False(service.Exists("key1"));
        
        File.Delete(tempPath);
    }

    [Fact]
    public void DatabaseService_GetKeysReturnsCorrectKeys()
    {
        var tempPath = Path.Combine(Path.GetTempPath(), $"test_db_{Guid.NewGuid()}.json");
        var service = new DatabaseService(
            Microsoft.Extensions.Logging.Abstractions.NullLogger<DatabaseService>.Instance,
            tempPath);

        service.Set("prefix_key1", "value1");
        service.Set("prefix_key2", "value2");
        service.Set("other_key", "value3");
        
        var prefixKeys = service.GetKeys("prefix_");
        
        Assert.Equal(2, prefixKeys.Count());
        Assert.Contains("prefix_key1", prefixKeys);
        Assert.Contains("prefix_key2", prefixKeys);
        
        File.Delete(tempPath);
    }

    [Fact]
    public void DatabaseService_ClearRemovesAllEntries()
    {
        var tempPath = Path.Combine(Path.GetTempPath(), $"test_db_{Guid.NewGuid()}.json");
        var service = new DatabaseService(
            Microsoft.Extensions.Logging.Abstractions.NullLogger<DatabaseService>.Instance,
            tempPath);

        service.Set("key1", "value1");
        service.Set("key2", "value2");
        service.Clear();
        
        Assert.False(service.Exists("key1"));
        Assert.False(service.Exists("key2"));
        
        File.Delete(tempPath);
    }
}
