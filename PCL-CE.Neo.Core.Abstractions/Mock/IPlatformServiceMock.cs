namespace PCL_CE.Neo.Core.Abstractions.Mock;

public class PlatformServiceMock : IPlatformService
{
    public string PlatformName { get; set; } = "TestPlatform";
    public string OSVersion { get; set; } = "TestOS 1.0";
    public string Architecture { get; set; } = "x64";
    
    public Action<string>? OnOpenUrl { get; set; }
    public Action<string>? OnOpenFolder { get; set; }
    
    public string LocalApplicationDataPath { get; set; } = Path.Combine(Path.GetTempPath(), "PCL_CE_Neo_Test");
    public string TemporaryPath { get; set; } = Path.Combine(Path.GetTempPath(), "PCL_CE_Neo_Test_Temp");
    public string GameDataPath { get; set; } = Path.Combine(Path.GetTempPath(), "PCL_CE_Neo_Test", "GameData");
    
    public void OpenUrl(string url)
    {
        OnOpenUrl?.Invoke(url);
    }

    public void OpenFolder(string path)
    {
        OnOpenFolder?.Invoke(path);
    }

    public string GetLocalApplicationDataPath()
    {
        return LocalApplicationDataPath;
    }

    public string GetTempPath()
    {
        return TemporaryPath;
    }

    public string GetGameDataPath()
    {
        return GameDataPath;
    }
}
