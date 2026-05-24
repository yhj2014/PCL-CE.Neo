namespace PCL_CE.Neo.Core.Minecraft;

public enum GameVersionType
{
    Release,
    Snapshot,
    OldAlpha,
    OldBeta
}

public record GameVersion(
    string Id,
    string Name,
    GameVersionType Type,
    DateTime ReleaseDate,
    string? ParentId = null
);

public record GameCore(
    string Id,
    string Name,
    string Type,
    bool IsModLoader,
    string? ModLoaderName = null
);

public record GameInstance(
    string Id,
    string Name,
    string GameCoreId,
    string? JavaPath,
    int MaxMemory,
    int MinMemory,
    string? JvmArguments,
    string? GameDirectory,
    string? AssetsDirectory,
    string? VersionDirectory
)
{
    public string WorkingDirectory => GameDirectory ?? Path.Combine(GetDefaultGameDir(), Id);
    
    public static string GetDefaultGameDir() 
    {
        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            ".minecraft"
        );
    }
}
