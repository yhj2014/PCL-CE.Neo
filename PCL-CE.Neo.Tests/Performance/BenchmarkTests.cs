using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using PCL_CE.Neo.Core.Configuration;
using PCL_CE.Neo.Core.Database;
using PCL_CE.Neo.Core.Logging;
using PCL_CE.Neo.Core.Network;
using PCL_CE.Neo.Core.IO;
using Xunit;

namespace PCL_CE.Neo.Tests.Performance;

public class StartupPerformanceTests
{
    [Fact]
    public async Task ServiceInitialization_CompletesWithinTimeLimit()
    {
        var stopwatch = Stopwatch.StartNew();
        
        var configService = new ConfigService(
            NullLogger<ConfigService>.Instance,
            Path.Combine(Path.GetTempPath(), $"test_config_{Guid.NewGuid()}.json"));
        
        await configService.InitializeAsync();
        
        stopwatch.Stop();
        
        Assert.True(stopwatch.ElapsedMilliseconds < 1000,
            $"Service initialization took {stopwatch.ElapsedMilliseconds}ms, expected < 1000ms");
    }

    [Fact]
    public async Task ConfigService_LoadPerformance()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"test_config_{Guid.NewGuid()}.json");
        var configService = new ConfigService(
            NullLogger<ConfigService>.Instance,
            tempFile);
        
        // Setup
        for (int i = 0; i < 100; i++)
        {
            configService.Set($"key_{i}", $"value_{i}");
        }
        await configService.SaveAsync();
        
        // Measure load time
        var stopwatch = Stopwatch.StartNew();
        await configService.LoadAsync();
        stopwatch.Stop();
        
        Assert.True(stopwatch.ElapsedMilliseconds < 500,
            $"Config load took {stopwatch.ElapsedMilliseconds}ms, expected < 500ms");
        
        File.Delete(tempFile);
    }

    [Fact]
    public async Task ConfigService_SavePerformance()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"test_config_{Guid.NewGuid()}.json");
        var configService = new ConfigService(
            NullLogger<ConfigService>.Instance,
            tempFile);
        
        // Setup with 100 entries
        for (int i = 0; i < 100; i++)
        {
            configService.Set($"key_{i}", $"value_{i}_with_some_extra_data_to_increase_size");
        }
        
        var stopwatch = Stopwatch.StartNew();
        await configService.SaveAsync();
        stopwatch.Stop();
        
        Assert.True(stopwatch.ElapsedMilliseconds < 1000,
            $"Config save took {stopwatch.ElapsedMilliseconds}ms, expected < 1000ms");
        
        File.Delete(tempFile);
    }
}

public class DatabasePerformanceTests
{
    [Fact]
    public async Task Database_WritePerformance()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"test_db_{Guid.NewGuid()}.json");
        var dbService = new DatabaseService(
            NullLogger<DatabaseService>.Instance,
            tempFile);
        
        var stopwatch = Stopwatch.StartNew();
        
        // Write 100 entries
        for (int i = 0; i < 100; i++)
        {
            dbService.Set($"key_{i}", $"value_{i}");
        }
        
        stopwatch.Stop();
        
        Assert.True(stopwatch.ElapsedMilliseconds < 1000,
            $"Database write (100 entries) took {stopwatch.ElapsedMilliseconds}ms, expected < 1000ms");
        
        File.Delete(tempFile);
    }

    [Fact]
    public async Task Database_ReadPerformance()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"test_db_{Guid.NewGuid()}.json");
        var dbService = new DatabaseService(
            NullLogger<DatabaseService>.Instance,
            tempFile);
        
        // Setup with 100 entries
        for (int i = 0; i < 100; i++)
        {
            dbService.Set($"key_{i}", $"value_{i}");
        }
        
        var stopwatch = Stopwatch.StartNew();
        
        // Read all entries
        for (int i = 0; i < 100; i++)
        {
            var value = dbService.Get<string>($"key_{i}");
            Assert.NotNull(value);
        }
        
        stopwatch.Stop();
        
        Assert.True(stopwatch.ElapsedMilliseconds < 100,
            $"Database read (100 entries) took {stopwatch.ElapsedMilliseconds}ms, expected < 100ms");
        
        File.Delete(tempFile);
    }

    [Fact]
    public async Task Database_ExistsCheckPerformance()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"test_db_{Guid.NewGuid()}.json");
        var dbService = new DatabaseService(
            NullLogger<DatabaseService>.Instance,
            tempFile);
        
        // Setup with 100 entries
        for (int i = 0; i < 100; i++)
        {
            dbService.Set($"key_{i}", $"value_{i}");
        }
        
        var stopwatch = Stopwatch.StartNew();
        
        // Check existence 100 times
        for (int i = 0; i < 100; i++)
        {
            var exists = dbService.Exists($"key_{i}");
            Assert.True(exists);
        }
        
        stopwatch.Stop();
        
        Assert.True(stopwatch.ElapsedMilliseconds < 50,
            $"Database exists check (100 times) took {stopwatch.ElapsedMilliseconds}ms, expected < 50ms");
        
        File.Delete(tempFile);
    }
}

public class NetworkPerformanceTests
{
    [Fact]
    public async Task NetworkService_TimeoutConfiguration()
    {
        var networkService = new NetworkService(NullLogger<NetworkService>.Instance);
        
        var stopwatch = Stopwatch.StartNew();
        
        // This should complete quickly due to timeout
        try
        {
            await networkService.GetAsync("https://invalid-domain-that-does-not-exist-12345.com");
        }
        catch
        {
            // Expected to fail
        }
        
        stopwatch.Stop();
        
        // Should timeout within reasonable time (30 seconds)
        Assert.True(stopwatch.ElapsedMilliseconds < 35000,
            $"Network timeout took {stopwatch.ElapsedMilliseconds}ms, expected < 35000ms");
    }

    [Fact]
    public void NetworkService_HttpClientReuse()
    {
        var networkService = new NetworkService(NullLogger<NetworkService>.Instance);
        
        // Create multiple clients
        var stopwatch = Stopwatch.StartNew();
        
        for (int i = 0; i < 10; i++)
        {
            var client = networkService.CreateHttpClient();
            Assert.NotNull(client);
        }
        
        stopwatch.Stop();
        
        Assert.True(stopwatch.ElapsedMilliseconds < 100,
            $"Creating 10 HttpClients took {stopwatch.ElapsedMilliseconds}ms, expected < 100ms");
    }
}

public class MemoryPerformanceTests
{
    [Fact]
    public void ConfigService_MemoryUsage()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"test_config_{Guid.NewGuid()}.json");
        var configService = new ConfigService(
            NullLogger<ConfigService>.Instance,
            tempFile);
        
        // Get initial memory
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        var initialMemory = GC.GetTotalMemory(true);
        
        // Add 1000 entries
        for (int i = 0; i < 1000; i++)
        {
            configService.Set($"key_{i}", $"value_{i}_with_some_extra_data");
        }
        
        // Force garbage collection
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        var finalMemory = GC.GetTotalMemory(true);
        
        var memoryIncrease = finalMemory - initialMemory;
        
        // Memory increase should be reasonable (less than 10MB for 1000 entries)
        Assert.True(memoryIncrease < 10 * 1024 * 1024,
            $"Memory increased by {memoryIncrease / 1024}KB, expected < 10MB");
        
        File.Delete(tempFile);
    }

    [Fact]
    public void DatabaseService_MemoryUsage()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"test_db_{Guid.NewGuid()}.json");
        var dbService = new DatabaseService(
            NullLogger<DatabaseService>.Instance,
            tempFile);
        
        // Get initial memory
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        var initialMemory = GC.GetTotalMemory(true);
        
        // Add 1000 entries
        for (int i = 0; i < 1000; i++)
        {
            dbService.Set($"key_{i}", $"value_{i}_with_some_extra_data_for_memory_testing");
        }
        
        // Force garbage collection
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        var finalMemory = GC.GetTotalMemory(true);
        
        var memoryIncrease = finalMemory - initialMemory;
        
        // Memory increase should be reasonable (less than 10MB for 1000 entries)
        Assert.True(memoryIncrease < 10 * 1024 * 1024,
            $"Memory increased by {memoryIncrease / 1024}KB, expected < 10MB");
        
        File.Delete(tempFile);
    }
}

public class BenchmarkSummary
{
    [Fact]
    public void PrintBenchmarkSummary()
    {
        // This test serves as documentation for expected performance
        Assert.True(true, @"
Performance Benchmarks Summary:
------------------------------
Startup Time: < 1000ms
Config Load (100 entries): < 500ms
Config Save (100 entries): < 1000ms
Database Write (100 entries): < 1000ms
Database Read (100 entries): < 100ms
Database Exists (100 checks): < 50ms
Network Timeout: < 35000ms
HttpClient Creation (10x): < 100ms
Memory (1000 config entries): < 10MB
Memory (1000 db entries): < 10MB
");
    }
}
