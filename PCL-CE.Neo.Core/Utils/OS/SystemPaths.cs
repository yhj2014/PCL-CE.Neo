using System;
using System.IO;

namespace PCL_CE.Neo.Core.Utils.OS;

public static class SystemPaths {
    public static string DriveLetter { get; } = Path.GetPathRoot(Environment.SystemDirectory)!;
}