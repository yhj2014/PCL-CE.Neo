using System;

namespace PCL_CE.Neo.Core.Minecraft;

public class MavenArtifact(string mavenId)
{
    public string Resolve(string uriOrPath)
    {
        return $"{uriOrPath.TrimEnd('/')}{_GetMavenPath(mavenId)}";
    }

    private static string _GetMavenPath(string packageId)
    {
        var packageIds = packageId.Split(":");
        switch (packageIds.Length)
        {
            case 3:
                return $"/{packageIds[0].Replace(".", "/")}/{packageIds[1]}/{packageIds[1]}-{packageIds[2]}.jar";
            case 4:
                if (_IsCommonPackaging(packageIds[2]))
                {
                    return $"/{packageIds[0].Replace(".", "/")}/{packageIds[1]}/{packageIds[1]}-{packageIds[3]}.{packageIds[2]}";
                }
                return $"/{packageIds[0].Replace(".", "/")}/{packageIds[1]}/{packageIds[1]}-{packageIds[2]}-{packageIds[3]}.jar";
            case 5:
                return $"/{packageIds[0].Replace(".", "/")}/{packageIds[1]}/{packageIds[3]}/{packageIds[1]}-{packageIds[3]}-{packageIds[4]}.{packageIds[2]}";
            default:
                throw new FormatException($"Invalid maven package id: Length is {packageIds.Length}");
        }
    }

    private static bool _IsCommonPackaging(string name)
    {
        return name == "jar" || name == "zip" || name == "pom";
    }
}