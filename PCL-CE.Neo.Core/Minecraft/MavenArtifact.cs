using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace PCL_CE.Neo.Core.Minecraft;

public class MavenArtifact
{
    private readonly ILogger<MavenArtifact> _logger;

    public string GroupId { get; }
    public string ArtifactId { get; }
    public string Version { get; }
    public string? Classifier { get; }
    public string Extension { get; }
    public string RepositoryUrl { get; }

    public MavenArtifact(string groupId, string artifactId, string version, string? classifier = null, 
        string extension = "jar", string repositoryUrl = "https://repo.maven.apache.org/maven2")
    {
        if (string.IsNullOrWhiteSpace(groupId))
            throw new ArgumentNullException(nameof(groupId));
        if (string.IsNullOrWhiteSpace(artifactId))
            throw new ArgumentNullException(nameof(artifactId));
        if (string.IsNullOrWhiteSpace(version))
            throw new ArgumentNullException(nameof(version));
        if (string.IsNullOrWhiteSpace(extension))
            throw new ArgumentNullException(nameof(extension));
        if (string.IsNullOrWhiteSpace(repositoryUrl))
            throw new ArgumentNullException(nameof(repositoryUrl));

        GroupId = groupId;
        ArtifactId = artifactId;
        Version = version;
        Classifier = classifier;
        Extension = extension;
        RepositoryUrl = repositoryUrl.TrimEnd('/');
    }

    public string GetPath()
    {
        var groupPath = GroupId.Replace('.', '/');
        var classifierPart = string.IsNullOrEmpty(Classifier) ? string.Empty : $"-{Classifier}";
        return $"{groupPath}/{ArtifactId}/{Version}/{ArtifactId}-{Version}{classifierPart}.{Extension}";
    }

    public string GetUrl()
    {
        return $"{RepositoryUrl}/{GetPath()}";
    }

    public string GetMetadataUrl()
    {
        var groupPath = GroupId.Replace('.', '/');
        return $"{RepositoryUrl}/{groupPath}/{ArtifactId}/{Version}/maven-metadata.xml";
    }

    public string GetLatestMetadataUrl()
    {
        var groupPath = GroupId.Replace('.', '/');
        return $"{RepositoryUrl}/{groupPath}/{ArtifactId}/maven-metadata.xml";
    }

    public bool IsSnapshotVersion()
    {
        return Version.EndsWith("-SNAPSHOT", StringComparison.OrdinalIgnoreCase);
    }

    public override string ToString()
    {
        var classifierPart = string.IsNullOrEmpty(Classifier) ? string.Empty : $":{Classifier}";
        return $"{GroupId}:{ArtifactId}:{Version}{classifierPart}:{Extension}";
    }

    public override bool Equals(object? obj)
    {
        if (obj is not MavenArtifact other)
            return false;

        return GroupId == other.GroupId &&
               ArtifactId == other.ArtifactId &&
               Version == other.Version &&
               Classifier == other.Classifier &&
               Extension == other.Extension;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(GroupId, ArtifactId, Version, Classifier, Extension);
    }

    public static MavenArtifact Parse(string mavenNotation)
    {
        if (string.IsNullOrWhiteSpace(mavenNotation))
            throw new ArgumentNullException(nameof(mavenNotation));

        var parts = mavenNotation.Split(':');
        if (parts.Length < 3 || parts.Length > 5)
            throw new FormatException("Invalid Maven notation format. Expected: groupId:artifactId:version[:classifier][:extension]");

        string groupId = parts[0];
        string artifactId = parts[1];
        string version = parts[2];
        string? classifier = parts.Length >= 4 ? parts[3] : null;
        string extension = parts.Length >= 5 ? parts[4] : "jar";

        return new MavenArtifact(groupId, artifactId, version, classifier, extension);
    }

    public static bool TryParse(string mavenNotation, out MavenArtifact? result)
    {
        result = null;

        try
        {
            result = Parse(mavenNotation);
            return true;
        }
        catch
        {
            return false;
        }
    }
}