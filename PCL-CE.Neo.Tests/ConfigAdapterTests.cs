using PCL_CE.Neo.Core.Configuration;
using Xunit;

namespace PCL_CE.Neo.Tests;

public class ConfigServiceTests
{
    [Fact]
    public void GetValue_ReturnsDefault_WhenKeyNotExists()
    {
        var service = new ConfigService();
        
        var result = service.GetValue("nonexistent", "default");
        
        Assert.Equal("default", result);
    }

    [Fact]
    public void SetValue_StoresValue()
    {
        var service = new ConfigService();
        
        service.SetValue("key", "value");
        
        Assert.Equal("value", service.GetValue("key", "default"));
    }

    [Fact]
    public void HasKey_ReturnsTrue_WhenKeyExists()
    {
        var service = new ConfigService();
        service.SetValue("key", "value");
        
        Assert.True(service.HasKey("key"));
    }

    [Fact]
    public void HasKey_ReturnsFalse_WhenKeyNotExists()
    {
        var service = new ConfigService();
        
        Assert.False(service.HasKey("nonexistent"));
    }

    [Fact]
    public void GetValue_ReturnsDefault_WhenWrongType()
    {
        var service = new ConfigService();
        service.SetValue("key", "value");
        
        var result = service.GetValue("key", 123);
        
        Assert.Equal(123, result);
    }

    [Fact]
    public void ConfigurationKeys_ContainsExpectedKeys()
    {
        Assert.NotEmpty(ConfigurationKeys.Theme);
        Assert.NotEmpty(ConfigurationKeys.Language);
        Assert.NotEmpty(ConfigurationKeys.GameDataPath);
        Assert.NotEmpty(ConfigurationKeys.JavaPath);
    }
}
