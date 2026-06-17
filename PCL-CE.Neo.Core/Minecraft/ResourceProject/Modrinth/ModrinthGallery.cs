namespace PCL_CE.Neo.Core.Minecraft.ResourceProject.Modrinth;

public record ModrinthGallery(
    string url,
    bool featured,
    string? title,
    string? description,
    string created,
    int ordering);