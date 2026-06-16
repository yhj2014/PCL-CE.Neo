using System.Collections.Generic;

namespace PCL_CE.Neo.Core.Minecraft.ResourceProject.Curseforge;

public record CurseforgeProjectResponse(CurseforgeProject data);
public record CurseforgeProjectsResponse(List<CurseforgeProject> data);