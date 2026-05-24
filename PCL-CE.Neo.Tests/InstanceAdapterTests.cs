using Xunit;
using PCL_CE.Neo.Core.Abstractions;

namespace PCL.CE.Neo.Tests;

public class InstanceAdapterTests
{
    [Fact]
    public void GameInstance_Properties()
    {
        var instance = new GameInstance
        {
            Id = "instance-123",
            Name = "Test Instance",
            Folder = "/tmp/instance",
            MinecraftVersion = "1.20.1",
            LoaderType = "Fabric",
            LoaderVersion = "0.15.1",
            CreatedAt = DateTime.Now,
            IsStarred = true,
            State = InstanceState.Ready
        };

        Assert.Equal("Test Instance", instance.Name);
        Assert.Equal("1.20.1", instance.MinecraftVersion);
        Assert.True(instance.IsStarred);
        Assert.Equal(InstanceState.Ready, instance.State);
    }

    [Fact]
    public void CreateInstanceOptions_Required()
    {
        var options = new CreateInstanceOptions
        {
            Name = "New Instance",
            MinecraftVersion = "1.19.4"
        };

        Assert.Equal("New Instance", options.Name);
        Assert.Equal("1.19.4", options.MinecraftVersion);
    }

    [Fact]
    public void InstanceState_Values()
    {
        Assert.Equal(InstanceState.Ready, InstanceState.Ready);
        Assert.Equal(InstanceState.Running, InstanceState.Running);
        Assert.Equal(InstanceState.Corrupted, InstanceState.Corrupted);
    }

    [Fact]
    public void GameInstance_CustomSettings()
    {
        var instance = new GameInstance
        {
            Id = "test",
            Name = "Test",
            Folder = "/tmp",
            MinecraftVersion = "1.20",
            CustomSettings = new Dictionary<string, string>
            {
                ["key1"] = "value1",
                ["key2"] = "value2"
            }
        };

        Assert.Equal(2, instance.CustomSettings.Count);
        Assert.Equal("value1", instance.CustomSettings["key1"]);
    }
}
