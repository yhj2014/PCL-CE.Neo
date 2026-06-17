using System;

namespace PCL_CE.Neo.Core.Minecraft.ResourceProject.Curseforge;

[Serializable]
public record CurseforgeScreenshots(
    int id,
    int modId,
    string title,
    string description,
    string thumbnailUrl);