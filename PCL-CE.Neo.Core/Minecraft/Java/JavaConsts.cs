using System.Collections.Generic;

namespace PCL_CE.Neo.Core.Minecraft.Java;

public static class JavaConsts
{
    public static readonly List<string> ExcludeFolderNames =
    [
        "Trash",
        "Recycler",
        "Recycle.Bin",
        "System Volume Information",
        "Temp",
        "tmp",
        ".Trash",
        ".DS_Store",
        "node_modules",
        "bin"
    ];
}