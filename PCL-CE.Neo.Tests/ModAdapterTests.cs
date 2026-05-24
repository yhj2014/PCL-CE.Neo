using Xunit;
using PCL_CE.Neo.Core.Abstractions;

namespace PCL_CE.Neo.Tests;

public class ModAdapterTests
{
    [Fact]
    public void ModInfo_Properties()
    {
        var modInfo = new ModInfo
        {
            Id = "mod-123",
            Name = "TestMod",
            FilePath = "/path/to/mod.jar",
            Source = ModSource.Local,
            Version = "1.0.0",
            IsEnabled = true,
            FileSize = 1024,
            FileHash = "abc123"
        };

        Assert.Equal("mod-123", modInfo.Id);
        Assert.Equal("TestMod", modInfo.Name);
        Assert.Equal("/path/to/mod.jar", modInfo.FilePath);
        Assert.Equal(ModSource.Local, modInfo.Source);
        Assert.Equal("1.0.0", modInfo.Version);
        Assert.True(modInfo.IsEnabled);
        Assert.Equal(1024, modInfo.FileSize);
        Assert.Equal("abc123", modInfo.FileHash);
    }

    [Fact]
    public void ModInfo_DefaultValues()
    {
        var modInfo = new ModInfo();

        Assert.Equal(ModSource.Local, modInfo.Source);
        Assert.True(modInfo.IsEnabled);
        Assert.Null(modInfo.RemoteId);
        Assert.Null(modInfo.Description);
        Assert.Null(modInfo.IconUrl);
    }

    [Fact]
    public void ModSource_AllTypes()
    {
        var sources = new List<ModSource>
        {
            ModSource.Local,
            ModSource.Modrinth,
            ModSource.CurseForge
        };

        Assert.Equal(3, sources.Count);
        Assert.Contains(ModSource.Local, Enum.GetValues<ModSource>());
        Assert.Contains(ModSource.Modrinth, Enum.GetValues<ModSource>());
        Assert.Contains(ModSource.CurseForge, Enum.GetValues<ModSource>());
    }

    [Fact]
    public void ModSearchQuery_Properties()
    {
        var query = new ModSearchQuery
        {
            Query = "fabric",
            MinecraftVersion = "1.20.1",
            LoaderType = "fabric",
            Source = ModSource.Modrinth,
            PageSize = 20,
            Offset = 0
        };

        Assert.Equal("fabric", query.Query);
        Assert.Equal("1.20.1", query.MinecraftVersion);
        Assert.Equal("fabric", query.LoaderType);
        Assert.Equal(ModSource.Modrinth, query.Source);
        Assert.Equal(20, query.PageSize);
        Assert.Equal(0, query.Offset);
    }

    [Fact]
    public void ModSearchQuery_DefaultValues()
    {
        var query = new ModSearchQuery();

        Assert.Equal(10, query.PageSize);
        Assert.Equal(0, query.Offset);
        Assert.Null(query.Query);
        Assert.Null(query.MinecraftVersion);
        Assert.Null(query.LoaderType);
        Assert.Null(query.Source);
    }

    [Fact]
    public void ModDownloadRequest_Properties()
    {
        var request = new ModDownloadRequest
        {
            InstanceId = "instance-123",
            ModId = "mod-456",
            Name = "TestMod",
            Url = "https://example.com/mod.jar",
            FileName = "testmod.jar",
            ExpectedHash = "hash123",
            Source = ModSource.Modrinth
        };

        Assert.Equal("instance-123", request.InstanceId);
        Assert.Equal("mod-456", request.ModId);
        Assert.Equal("TestMod", request.Name);
        Assert.Equal("https://example.com/mod.jar", request.Url);
        Assert.Equal("testmod.jar", request.FileName);
        Assert.Equal("hash123", request.ExpectedHash);
        Assert.Equal(ModSource.Modrinth, request.Source);
    }

    [Fact]
    public void DownloadResult_Success()
    {
        var result = DownloadResult.Success(1024);

        Assert.True(result.Success);
        Assert.Equal(1024, result.BytesDownloaded);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public void DownloadResult_Failure()
    {
        var result = DownloadResult.Failed("Network error");

        Assert.False(result.Success);
        Assert.Equal(0, result.BytesDownloaded);
        Assert.Equal("Network error", result.ErrorMessage);
    }

    [Fact]
    public void ModUpdateInfo_Properties()
    {
        var updateInfo = new ModUpdateInfo
        {
            ModId = "mod-123",
            CurrentVersion = "1.0.0",
            LatestVersion = "1.1.0",
            IsUpdateAvailable = true,
            DownloadUrl = "https://example.com/new.jar"
        };

        Assert.Equal("mod-123", updateInfo.ModId);
        Assert.Equal("1.0.0", updateInfo.CurrentVersion);
        Assert.Equal("1.1.0", updateInfo.LatestVersion);
        Assert.True(updateInfo.IsUpdateAvailable);
        Assert.Equal("https://example.com/new.jar", updateInfo.DownloadUrl);
    }

    [Fact]
    public void ModUpdateInfo_NoUpdate()
    {
        var updateInfo = new ModUpdateInfo
        {
            ModId = "mod-123",
            CurrentVersion = "1.0.0",
            IsUpdateAvailable = false
        };

        Assert.False(updateInfo.IsUpdateAvailable);
        Assert.Equal(updateInfo.CurrentVersion, updateInfo.LatestVersion);
    }

    [Fact]
    public void ModInfo_WithOptionalProperties()
    {
        var modInfo = new ModInfo
        {
            Id = "mod-123",
            Name = "AdvancedMod",
            FilePath = "/path/to/mod.jar",
            Source = ModSource.Modrinth,
            RemoteId = "modrinth-id-456",
            Version = "2.0.0",
            Description = "A very advanced mod",
            IconUrl = "https://example.com/icon.png",
            FileSize = 2048,
            FileHash = "def456",
            IsEnabled = true
        };

        Assert.Equal("modrinth-id-456", modInfo.RemoteId);
        Assert.Equal("A very advanced mod", modInfo.Description);
        Assert.Equal("https://example.com/icon.png", modInfo.IconUrl);
    }

    [Fact]
    public void ModSource_CurseForge()
    {
        var modInfo = new ModInfo
        {
            Id = "curseforge-mod",
            Name = "CurseMod",
            Source = ModSource.CurseForge,
            RemoteId = "curse-id-789"
        };

        Assert.Equal(ModSource.CurseForge, modInfo.Source);
        Assert.Equal("curse-id-789", modInfo.RemoteId);
    }

    [Fact]
    public void ModSearchQuery_ModrinthSearch()
    {
        var query = new ModSearchQuery
        {
            Query = "optifine",
            MinecraftVersion = "1.19.2",
            LoaderType = "forge",
            Source = ModSource.Modrinth
        };

        Assert.Equal("optifine", query.Query);
        Assert.Equal("1.19.2", query.MinecraftVersion);
        Assert.Equal("forge", query.LoaderType);
        Assert.Equal(ModSource.Modrinth, query.Source);
    }

    [Fact]
    public void ModSearchQuery_CurseForgeSearch()
    {
        var query = new ModSearchQuery
        {
            Query = "journeymap",
            MinecraftVersion = "1.18.2",
            Source = ModSource.CurseForge
        };

        Assert.Equal("journeymap", query.Query);
        Assert.Equal("1.18.2", query.MinecraftVersion);
        Assert.Equal(ModSource.CurseForge, query.Source);
    }
}
