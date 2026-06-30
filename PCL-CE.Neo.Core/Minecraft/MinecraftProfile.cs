using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace PCL_CE.Neo.Core.Minecraft;

public class MinecraftProfile
{
    private readonly ILogger<MinecraftProfile> _logger;

    public string Id { get; }
    public string Name { get; }
    public string Type { get; }
    public string Version { get; }
    public string GameDir { get; }
    public string AssetsDir { get; }
    public string LibraryDir { get; }
    public string LoggingDir { get; }
    public string NativesDir { get; }
    public string MainClass { get; }
    public string? JavaPath { get; set; }
    public IList<string> JvmArguments { get; } = new List<string>();
    public IList<string> GameArguments { get; } = new List<string>();
    public IList<string> Libraries { get; } = new List<string>();
    public IDictionary<string, string> ResolvedPaths { get; } = new Dictionary<string, string>();

    public MinecraftProfile(string id, string name, string type, string version, 
        string gameDir, string assetsDir, string libraryDir, string loggingDir, 
        string nativesDir, string mainClass, ILogger<MinecraftProfile> logger)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Type = type ?? throw new ArgumentNullException(nameof(type));
        Version = version ?? throw new ArgumentNullException(nameof(version));
        GameDir = gameDir ?? throw new ArgumentNullException(nameof(gameDir));
        AssetsDir = assetsDir ?? throw new ArgumentNullException(nameof(assetsDir));
        LibraryDir = libraryDir ?? throw new ArgumentNullException(nameof(libraryDir));
        LoggingDir = loggingDir ?? throw new ArgumentNullException(nameof(loggingDir));
        NativesDir = nativesDir ?? throw new ArgumentNullException(nameof(nativesDir));
        MainClass = mainClass ?? throw new ArgumentNullException(nameof(mainClass));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void AddJvmArgument(string argument)
    {
        if (!string.IsNullOrEmpty(argument))
        {
            JvmArguments.Add(argument);
        }
    }

    public void AddGameArgument(string argument)
    {
        if (!string.IsNullOrEmpty(argument))
        {
            GameArguments.Add(argument);
        }
    }

    public void AddLibrary(string library)
    {
        if (!string.IsNullOrEmpty(library) && !Libraries.Contains(library))
        {
            Libraries.Add(library);
        }
    }

    public string BuildJavaCommand()
    {
        var javaExe = JavaPath ?? "java";
        var classPath = string.Join(Path.PathSeparator, Libraries);
        var jvmArgs = string.Join(" ", JvmArguments);
        var gameArgs = string.Join(" ", GameArguments);

        return $"{javaExe} {jvmArgs} -cp {classPath} {MainClass} {gameArgs}";
    }

    public IDictionary<string, string> GetEnvironmentVariables()
    {
        return new Dictionary<string, string>
        {
            { "JAVA_HOME", Path.GetDirectoryName(Path.GetDirectoryName(JavaPath)) ?? string.Empty },
            { "MINECRAFT_VERSION", Version },
            { "GAMEDIR", GameDir },
            { "ASSETSDIR", AssetsDir },
            { "LIBRARYDIR", LibraryDir },
            { "NATIVESDIR", NativesDir }
        };
    }

    public bool Validate()
    {
        var errors = new List<string>();

        if (!Directory.Exists(GameDir))
            errors.Add($"Game directory not found: {GameDir}");

        if (!Directory.Exists(AssetsDir))
            errors.Add($"Assets directory not found: {AssetsDir}");

        if (!Directory.Exists(LibraryDir))
            errors.Add($"Library directory not found: {LibraryDir}");

        if (!Directory.Exists(NativesDir))
            errors.Add($"Natives directory not found: {NativesDir}");

        if (string.IsNullOrEmpty(JavaPath) || !File.Exists(JavaPath))
            errors.Add($"Java executable not found: {JavaPath}");

        if (errors.Any())
        {
            _logger.LogWarning("Profile validation failed for {ProfileName}: {Errors}", Name, string.Join(", ", errors));
            return false;
        }

        _logger.LogDebug("Profile validation passed for {ProfileName}", Name);
        return true;
    }

    public void ResolvePlaceholders(IDictionary<string, string> replacements)
    {
        if (replacements == null)
            return;

        foreach (var replacement in replacements)
        {
            for (int i = 0; i < JvmArguments.Count; i++)
            {
                JvmArguments[i] = JvmArguments[i].Replace($"${{{replacement.Key}}}", replacement.Value);
            }

            for (int i = 0; i < GameArguments.Count; i++)
            {
                GameArguments[i] = GameArguments[i].Replace($"${{{replacement.Key}}}", replacement.Value);
            }
        }
    }

    public string ToJson()
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        return JsonSerializer.Serialize(this, options);
    }

    public static MinecraftProfile? FromJson(string json, ILogger<MinecraftProfile> logger)
    {
        try
        {
            return JsonSerializer.Deserialize<MinecraftProfile>(json);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to deserialize Minecraft profile from JSON");
            return null;
        }
    }
}