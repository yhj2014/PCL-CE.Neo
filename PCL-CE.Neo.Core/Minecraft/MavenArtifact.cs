using System.Text.RegularExpressions;

namespace PCL_CE.Neo.Core.Minecraft;

public class MavenArtifact
{
    public string GroupId { get; }
    public string ArtifactId { get; }
    public string Version { get; }
    public string? Classifier { get; }
    public string Extension { get; }

    private static readonly Regex _MavenRegex = new(
        @"^(?<groupId>[^:]+):(?<artifactId>[^:]+)(:(?<classifier>[^:]+))?(:(?<extension>[^:]+))?:(?<version>[^:]+)$",
        RegexOptions.Compiled);

    public MavenArtifact(string groupId, string artifactId, string version, string? classifier = null, string extension = "jar")
    {
        GroupId = groupId;
        ArtifactId = artifactId;
        Version = version;
        Classifier = classifier;
        Extension = extension;
    }

    public static bool TryParse(string input, out MavenArtifact? artifact)
    {
        artifact = null;

        if (string.IsNullOrWhiteSpace(input))
            return false;

        var match = _MavenRegex.Match(input);
        if (!match.Success)
            return false;

        var groupId = match.Groups["groupId"].Value;
        var artifactId = match.Groups["artifactId"].Value;
        var version = match.Groups["version"].Value;
        var classifier = match.Groups["classifier"].Success ? match.Groups["classifier"].Value : null;
        var extension = match.Groups["extension"].Success ? match.Groups["extension"].Value : "jar";

        artifact = new MavenArtifact(groupId, artifactId, version, classifier, extension);
        return true;
    }

    public static MavenArtifact Parse(string input)
    {
        if (!TryParse(input, out var artifact))
            throw new FormatException("Invalid Maven artifact format");

        return artifact!;
    }

    public string GetPath()
    {
        var path = $"{GroupId.Replace('.', '/')}/{ArtifactId}/{Version}/{ArtifactId}-{Version}";
        if (!string.IsNullOrEmpty(Classifier))
            path += $"-{Classifier}";
        path += $".{Extension}";
        return path;
    }

    public string GetFileName()
    {
        var name = $"{ArtifactId}-{Version}";
        if (!string.IsNullOrEmpty(Classifier))
            name += $"-{Classifier}";
        name += $".{Extension}";
        return name;
    }

    public override string ToString()
    {
        var result = $"{GroupId}:{ArtifactId}";
        if (!string.IsNullOrEmpty(Classifier))
            result += $":{Classifier}";
        result += $":{Extension}:{Version}";
        return result;
    }
}