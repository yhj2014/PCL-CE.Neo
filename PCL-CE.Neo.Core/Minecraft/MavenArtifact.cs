using System;
using System.Collections.Generic;
using System.Linq;

namespace PCL_CE.Neo.Core.Minecraft;

public class MavenArtifact
{
    public string GroupId { get; }
    public string ArtifactId { get; }
    public string Version { get; }
    public string? Classifier { get; }
    public string Extension { get; } = "jar";

    public MavenArtifact(string groupId, string artifactId, string version, string? classifier = null, string extension = "jar")
    {
        GroupId = groupId ?? throw new ArgumentNullException(nameof(groupId));
        ArtifactId = artifactId ?? throw new ArgumentNullException(nameof(artifactId));
        Version = version ?? throw new ArgumentNullException(nameof(version));
        Classifier = classifier;
        Extension = extension;
    }

    public string GetPath()
    {
        var groupPath = GroupId.Replace('.', '/');
        var classifierPart = string.IsNullOrEmpty(Classifier) ? "" : $"-{Classifier}";
        return $"{groupPath}/{ArtifactId}/{Version}/{ArtifactId}-{Version}{classifierPart}.{Extension}";
    }

    public string GetFileName()
    {
        var classifierPart = string.IsNullOrEmpty(Classifier) ? "" : $"-{Classifier}";
        return $"{ArtifactId}-{Version}{classifierPart}.{Extension}";
    }

    public string GetUrl(string repositoryUrl)
    {
        var baseUrl = repositoryUrl.TrimEnd('/');
        return $"{baseUrl}/{GetPath()}";
    }

    public static bool TryParse(string mavenNotation, out MavenArtifact? artifact)
    {
        artifact = null;
        
        if (string.IsNullOrWhiteSpace(mavenNotation))
            return false;

        var parts = mavenNotation.Split(':');
        
        if (parts.Length < 3 || parts.Length > 5)
            return false;

        var groupId = parts[0].Trim();
        var artifactId = parts[1].Trim();
        var version = parts[2].Trim();
        string? classifier = null;
        string extension = "jar";

        if (parts.Length == 4)
        {
            if (parts[3].Contains('@'))
            {
                var extParts = parts[3].Split('@');
                classifier = extParts[0];
                extension = extParts.Length > 1 ? extParts[1] : "jar";
            }
            else
            {
                extension = parts[3];
            }
        }
        else if (parts.Length == 5)
        {
            classifier = parts[3];
            extension = parts[4];
        }

        if (string.IsNullOrWhiteSpace(groupId) || string.IsNullOrWhiteSpace(artifactId) || string.IsNullOrWhiteSpace(version))
            return false;

        artifact = new MavenArtifact(groupId, artifactId, version, classifier, extension);
        return true;
    }

    public static MavenArtifact Parse(string mavenNotation)
    {
        if (!TryParse(mavenNotation, out var artifact))
            throw new FormatException($"Invalid Maven notation: {mavenNotation}");
        return artifact!;
    }

    public override string ToString()
    {
        if (string.IsNullOrEmpty(Classifier))
        {
            if (Extension == "jar")
                return $"{GroupId}:{ArtifactId}:{Version}";
            return $"{GroupId}:{ArtifactId}:{Version}:{Extension}";
        }
        return $"{GroupId}:{ArtifactId}:{Version}:{Classifier}:{Extension}";
    }

    public override bool Equals(object? obj)
    {
        return obj is MavenArtifact other &&
               GroupId == other.GroupId &&
               ArtifactId == other.ArtifactId &&
               Version == other.Version &&
               Classifier == other.Classifier &&
               Extension == other.Extension;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(GroupId, ArtifactId, Version, Classifier, Extension);
    }
}