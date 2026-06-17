namespace PCL_CE.Neo.Core.Minecraft.ResourceProject.Curseforge;

public record CurseforgeCategories(
    int id,
    int gameId,
    string name,
    string slug,
    string url,
    string iconUrl,
    string dateModified,
    bool isClass,
    int classId,
    int parentCategoryId,
    int displayIndex);