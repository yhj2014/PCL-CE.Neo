using Microsoft.Extensions.Logging;
using PCL_CE.Neo.Core.Abstractions;
using PCL_CE.Neo.Core.Adapters;
using Xunit;

namespace PCL_CE.Neo.Tests;

public class AdapterTests
{
    [Fact]
    public void ConfigAdapter_StoresAndRetrievesValues()
    {
        var adapter = new ConfigAdapter();
        
        adapter.SetConfig("test_key", "test_value");
        
        var result = adapter.GetConfig("test_key", "default");
        Assert.Equal("test_value", result);
    }

    [Fact]
    public void ConfigAdapter_ReturnsDefaultWhenKeyNotExists()
    {
        var adapter = new ConfigAdapter();
        
        var result = adapter.GetConfig("nonexistent", "default_value");
        
        Assert.Equal("default_value", result);
    }

    [Fact]
    public void PathsAdapter_ReturnsValidPaths()
    {
        var adapter = new PathsAdapter();
        
        Assert.False(string.IsNullOrEmpty(adapter.Data));
        Assert.False(string.IsNullOrEmpty(adapter.SharedData));
        Assert.False(string.IsNullOrEmpty(adapter.Temp));
    }

    [Fact]
    public void LoggerAdapter_CanBeInstantiated()
    {
        var adapter = new LoggerAdapter();
        
        adapter.LogDebug("Test debug message");
        adapter.LogInfo("Test info message");
        adapter.LogWarning("Test warning message");
        adapter.LogError("Test error message");
    }

    [Fact]
    public void StateAdapter_InitializesCorrectly()
    {
        var adapter = new StateAdapter();
        
        Assert.NotNull(adapter.State);
    }

    [Fact]
    public void TelemetryAdapter_CanBeInstantiated()
    {
        var adapter = new TelemetryAdapter();
        
        Assert.NotNull(adapter);
    }

    [Fact]
    public void ResourceDownloadAdapter_CanBeInstantiated()
    {
        var adapter = new ResourceDownloadAdapter();

        Assert.NotNull(adapter);
    }
}
