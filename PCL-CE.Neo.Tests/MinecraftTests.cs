using PCL_CE.Neo.Core.Minecraft;
using Xunit;

namespace PCL_CE.Neo.Tests;

public class MinecraftTests
{
    [Fact]
    public void GameVersion_CanBeCreated()
    {
        var version = new GameVersion("1.20.1", "Minecraft 1.20.1", GameVersionType.Release, DateTime.Now);
        
        Assert.Equal("1.20.1", version.Id);
        Assert.Equal("Minecraft 1.20.1", version.Name);
        Assert.Equal(GameVersionType.Release, version.Type);
    }

    [Fact]
    public void GameCore_CanBeCreated()
    {
        var core = new GameCore("1.20.1-forge", "Forge 1.20.1", "forge", true, "Forge");
        
        Assert.Equal("1.20.1-forge", core.Id);
        Assert.True(core.IsModLoader);
        Assert.Equal("Forge", core.ModLoaderName);
    }

    [Fact]
    public void GameInstance_DefaultWorkingDirectory()
    {
        var instance = new GameInstance(
            "test-instance",
            "Test Instance",
            "1.20.1",
            null,
            4096,
            1024,
            null,
            null,
            null,
            null
        );
        
        Assert.Contains("test-instance", instance.WorkingDirectory);
    }

    [Fact]
    public void JavaInstallation_CanBeCreated()
    {
        var java = new JavaInstallation(
            "/usr/bin/java",
            "17.0.1",
            "Oracle Corporation",
            4096,
            JavaBrandType.Oracle
        );
        
        Assert.Equal("/usr/bin/java", java.Path);
        Assert.Equal("17.0.1", java.Version);
        Assert.Equal(JavaBrandType.Oracle, java.Brand);
    }

    [Fact]
    public void LaunchOptions_EnableInnocence_DefaultsFalse()
    {
        var options = new LaunchOptions(
            "/game",
            "/java",
            4096,
            1024
        );
        
        Assert.False(options.EnableInnocence);
    }
}
