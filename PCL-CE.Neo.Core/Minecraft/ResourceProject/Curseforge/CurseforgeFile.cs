namespace PCL_CE.Neo.Core.Minecraft.ResourceProject.Curseforge;

public record CurseforgeFile(
    int id,
    int gameId,
    int modId,
    bool isAvailable,
    string displayName,
    string fileName,
    int releaseType,
    int fileStatus,
    CurseforgeHashes hashes);