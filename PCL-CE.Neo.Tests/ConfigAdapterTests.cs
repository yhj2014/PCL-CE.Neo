using PCL_CE.Neo.Core;
using PCL_CE.Neo.Core.Abstractions;

namespace PCL.CE.Neo.Tests;

public class ConfigAdapterTests
{
    [Fact]
    public void GetConfig_ReturnsDefault_WhenKeyNotExists()
    {
        var logger = new TestLogger<Adapters.ConfigAdapter>();
        var paths = new Adapters.PathsAdapter(logger, new Adapters.ApplicationAdapter(logger, new Platform.Windows.WindowsPlatformService()));
        var config = new Adapters.ConfigAdapter(logger, paths);

        var result = config.GetConfig("NonExistentKey", "default");
        Assert.Equal("default", result);
    }

    [Fact]
    public void SetConfig_StoresValue()
    {
        var logger = new TestLogger<Adapters.ConfigAdapter>();
        var paths = new Adapters.PathsAdapter(logger, new Adapters.ApplicationAdapter(logger, new Platform.Windows.WindowsPlatformService()));
        var config = new Adapters.ConfigAdapter(logger, paths);

        config.SetConfig("TestKey", "TestValue");

        var result = config.GetConfig("TestKey", "");
        Assert.Equal("TestValue", result);
    }

    [Fact]
    public void HasConfig_ReturnsFalse_WhenKeyNotExists()
    {
        var logger = new TestLogger<Adapters.ConfigAdapter>();
        var paths = new Adapters.PathsAdapter(logger, new Adapters.ApplicationAdapter(logger, new Platform.Windows.WindowsPlatformService()));
        var config = new Adapters.ConfigAdapter(logger, paths);

        Assert.False(config.HasConfig("NonExistentKey"));
    }

    [Fact]
    public void HasConfig_ReturnsTrue_WhenKeyExists()
    {
        var logger = new TestLogger<Adapters.ConfigAdapter>();
        var paths = new Adapters.PathsAdapter(logger, new Adapters.ApplicationAdapter(logger, new Platform.Windows.WindowsPlatformService()));
        var config = new Adapters.ConfigAdapter(logger, paths);

        config.SetConfig("ExistingKey", "Value");

        Assert.True(config.HasConfig("ExistingKey"));
    }

    [Fact]
    public void GetConfig_ReturnsDefaultInt()
    {
        var logger = new TestLogger<Adapters.ConfigAdapter>();
        var paths = new Adapters.PathsAdapter(logger, new Adapters.ApplicationAdapter(logger, new Platform.Windows.WindowsPlatformService()));
        var config = new Adapters.ConfigAdapter(logger, paths);

        var result = config.GetConfig("IntKey", 42);
        Assert.Equal(42, result);
    }

    [Fact]
    public void GetConfig_ReturnsDefaultBool()
    {
        var logger = new TestLogger<Adapters.ConfigAdapter>();
        var paths = new Adapters.PathsAdapter(logger, new Adapters.ApplicationAdapter(logger, new Platform.Windows.WindowsPlatformService()));
        var config = new Adapters.ConfigAdapter(logger, paths);

        var result = config.GetConfig("BoolKey", true);
        Assert.True(result);
    }
}
