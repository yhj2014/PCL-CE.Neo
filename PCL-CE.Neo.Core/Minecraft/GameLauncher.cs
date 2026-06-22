using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using PCL_CE.Neo.Core.Abstractions;
using PCL_CE.Neo.Core.IO;
using PCL_CE.Neo.Core.Network;

namespace PCL_CE.Neo.Core.Minecraft;

public interface IGameLauncher
{
    Task<Process> LaunchGameAsync(GameInstance instance, LaunchOptions options);
    event Action<string>? GameOutput;
    event Action<int>? GameExited;
}

public record LaunchOptions(
    string GameDirectory,
    string JavaPath,
    int MaxMemory,
    int MinMemory,
    string? JvmArguments = null,
    string? GameArguments = null,
    string? AssetsDirectory = null,
    string? NativeDirectory = null,
    bool EnableInnocence = false
);

public class GameLauncher : IGameLauncher
{
    private readonly ILogger<GameLauncher> _logger;
    private readonly INetworkService _networkService;
    private readonly IDownloadService _downloadService;
    private readonly IPathsAdapter _pathsAdapter;

    public event Action<string>? GameOutput;
    public event Action<int>? GameExited;

    public GameLauncher() : this(
        Microsoft.Extensions.Logging.Abstractions.NullLogger<GameLauncher>.Instance,
        new PCL_CE.Neo.Core.Network.NetworkService(),
        new DownloadService(),
        new PathsAdapter())
    {
    }

    public GameLauncher(ILogger<GameLauncher> logger, INetworkService networkService, IDownloadService downloadService, IPathsAdapter pathsAdapter)
    {
        _logger = logger;
        _networkService = networkService;
        _downloadService = downloadService;
        _pathsAdapter = pathsAdapter;
    }

    public async Task<Process> LaunchGameAsync(GameInstance instance, LaunchOptions options)
    {
        _logger.LogInformation("Launching game {InstanceName} with Java {JavaPath}", 
            instance.Name, options.JavaPath);

        var processInfo = CreateProcess(instance, options);
        
        try
        {
            var process = Process.Start(processInfo);
            if (process == null)
            {
                throw new InvalidOperationException("Failed to start game process");
            }

            process.EnableRaisingEvents = true;
            process.OutputDataReceived += (s, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    GameOutput?.Invoke(e.Data);
                }
            };
            process.ErrorDataReceived += (s, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    GameOutput?.Invoke($"[ERROR] {e.Data}");
                }
            };
            
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            
            process.Exited += (s, e) =>
            {
                GameExited?.Invoke(process.ExitCode);
            };

            _logger.LogInformation("Game process started with PID {ProcessId}", process.Id);
            return process;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to launch game");
            throw;
        }
    }

    private ProcessStartInfo CreateProcess(GameInstance instance, LaunchOptions options)
    {
        var javaPath = options.JavaPath;
        var gameDir = options.GameDirectory;
        var assetsDir = options.AssetsDirectory ?? Path.Combine(gameDir, "assets");
        var nativesDir = options.NativeDirectory ?? Path.Combine(gameDir, "natives");
        var librariesDir = Path.Combine(gameDir, "libraries");
        var versionsDir = Path.Combine(gameDir, "versions", instance.GameCoreId);

        // Build classpath with libraries
        var classPath = BuildClassPath(librariesDir, versionsDir, instance.GameCoreId);
        
        // Get assets index from version JSON
        var assetsIndex = GetAssetsIndex(gameDir, instance.GameCoreId);
        
        // Get version type from version JSON
        var versionType = GetVersionType(gameDir, instance.GameCoreId);

        var arguments = new List<string>();
        
        // JVM memory arguments
        arguments.Add($"-Xms{options.MinMemory}M");
        arguments.Add($"-Xmx{options.MaxMemory}M");
        
        // Additional JVM arguments
        if (!string.IsNullOrEmpty(options.JvmArguments))
        {
            arguments.Add(options.JvmArguments);
        }

        // Minecraft-specific JVM arguments
        arguments.Add($"-Djava.library.path=\"{nativesDir}\"");
        arguments.Add($"-Dminecraft.client.jar=\"{versionsDir}\\{instance.GameCoreId}.jar\"");
        arguments.Add($"-Dminecraft.assets.root=\"{assetsDir}\"");
        arguments.Add($"-Dminecraft.assets.index={assetsIndex}");

        // Classpath
        arguments.Add("-cp");
        arguments.Add(classPath);

        // Main class
        arguments.Add("net.minecraft.client.main.Main");
        
        // Game arguments
        arguments.Add($"--username ${{username}}");
        arguments.Add($"--version ${{version}}");
        arguments.Add($"--gameDir \"{gameDir}\"");
        arguments.Add($"--assetsDir \"{assetsDir}\"");
        arguments.Add($"--assetIndex ${{assetIndex}}");
        arguments.Add($"--uuid ${{uuid}}");
        arguments.Add($"--accessToken ${{accessToken}}");
        arguments.Add($"--clientId ${{clientId}}");
        arguments.Add($"--xuid ${{xuid}}");
        arguments.Add($"--userType ${{userType}}");
        arguments.Add($"--versionType {versionType}");

        if (options.EnableInnocence)
        {
            arguments.Add("-- Innocence");
        }

        var argumentString = string.Join(" ", arguments);

        _logger.LogDebug("Java arguments: {Arguments}", argumentString);

        return new ProcessStartInfo
        {
            FileName = javaPath,
            Arguments = argumentString,
            WorkingDirectory = gameDir,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };
    }

    private string BuildClassPath(string librariesDir, string versionsDir, string gameCoreId)
    {
        var pathSeparator = Path.PathSeparator;
        var classPath = new List<string>();
        
        // Add version JAR
        var versionJar = Path.Combine(versionsDir, $"{gameCoreId}.jar");
        if (File.Exists(versionJar))
        {
            classPath.Add(versionJar);
            _logger.LogDebug("Added version JAR: {Path}", versionJar);
        }
        else
        {
            _logger.LogWarning("Version JAR not found: {Path}", versionJar);
        }
        
        // Add all libraries from the libraries directory
        if (Directory.Exists(librariesDir))
        {
            var libraryFiles = Directory.GetFiles(librariesDir, "*.jar", SearchOption.AllDirectories);
            foreach (var lib in libraryFiles)
            {
                classPath.Add(lib);
                _logger.LogDebug("Added library: {Path}", lib);
            }
        }
        else
        {
            _logger.LogWarning("Libraries directory not found: {Path}", librariesDir);
        }

        var result = string.Join(pathSeparator.ToString(), classPath);
        _logger.LogDebug("Built classpath with {Count} entries", classPath.Count);
        return result;
    }

    private string GetAssetsIndex(string gameDir, string gameCoreId)
    {
        var versionJsonPath = Path.Combine(gameDir, "versions", gameCoreId, $"{gameCoreId}.json");
        
        if (!File.Exists(versionJsonPath))
        {
            _logger.LogWarning("Version JSON not found: {Path}, using default assets index", versionJsonPath);
            return gameCoreId;
        }
        
        try
        {
            var jsonContent = File.ReadAllText(versionJsonPath);
            using var doc = JsonDocument.Parse(jsonContent);
            
            if (doc.RootElement.TryGetProperty("assets", out var assetsElement))
            {
                var assets = assetsElement.GetString();
                _logger.LogDebug("Got assets index from version JSON: {Assets}", assets);
                return assets ?? gameCoreId;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse version JSON for assets index");
        }
        
        return gameCoreId;
    }

    private string GetVersionType(string gameDir, string gameCoreId)
    {
        var versionJsonPath = Path.Combine(gameDir, "versions", gameCoreId, $"{gameCoreId}.json");
        
        if (!File.Exists(versionJsonPath))
        {
            return "release";
        }
        
        try
        {
            var jsonContent = File.ReadAllText(versionJsonPath);
            using var doc = JsonDocument.Parse(jsonContent);
            
            if (doc.RootElement.TryGetProperty("type", out var typeElement))
            {
                var type = typeElement.GetString();
                _logger.LogDebug("Got version type from version JSON: {Type}", type);
                return type ?? "release";
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse version JSON for version type");
        }
        
        return "release";
    }
}
