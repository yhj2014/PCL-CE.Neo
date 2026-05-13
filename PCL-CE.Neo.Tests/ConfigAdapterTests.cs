using Xunit;
using PCL_CE.Neo.Core.Abstractions;
using PCL_CE.Neo.Core.Adapters;

namespace PCL.CE.Neo.Tests;

public class ConfigAdapterTests
{
    [Fact]
    public void GetConfig_ReturnsDefault_WhenKeyNotExists()
    {
        var config = new ConfigAdapter();
        var result = config.GetConfig("NonExistentKey", "default");
        Assert.Equal("default", result);
    }

    [Fact]
    public void SetConfig_StoresValue()
    {
        var config = new ConfigAdapter();
        config.SetConfig("TestKey", "TestValue");
        var result = config.GetConfig("TestKey", "");
        Assert.Equal("TestValue", result);
    }

    [Fact]
    public void HasConfig_ReturnsFalse_WhenKeyNotExists()
    {
        var config = new ConfigAdapter();
        Assert.False(config.HasConfig("NonExistentKey"));
    }

    [Fact]
    public void HasConfig_ReturnsTrue_WhenKeyExists()
    {
        var config = new ConfigAdapter();
        config.SetConfig("ExistingKey", "Value");
        Assert.True(config.HasConfig("ExistingKey"));
    }

    [Fact]
    public void GetConfig_ReturnsDefaultInt()
    {
        var config = new ConfigAdapter();
        var result = config.GetConfig("IntKey", 42);
        Assert.Equal(42, result);
    }

    [Fact]
    public void GetConfig_ReturnsDefaultBool()
    {
        var config = new ConfigAdapter();
        var result = config.GetConfig("BoolKey", true);
        Assert.True(result);
    }
}
