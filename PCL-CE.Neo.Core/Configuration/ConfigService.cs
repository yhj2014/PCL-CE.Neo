using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PCL_CE.Neo.Core.App;
using PCL_CE.Neo.Core.Configuration.Storage;

namespace PCL_CE.Neo.Core.Configuration;

public sealed partial class ConfigService
{
    private static readonly Dictionary<string, ConfigItem> _Items = [];
    private static readonly HashSet<string> _KeySet = [];

    public static IReadOnlySet<string> KeySet => _KeySet;

    [ConfigItem<int>("FileVersion", 1)] public static partial int SharedVersion { get; set; }
    [ConfigItem<int>("LocalFileVersion", 1, ConfigSource.Local)] public static partial int LocalVersion { get; set; }

    public static string SharedConfigPath { get; } = Path.Combine(Paths.SharedData, "config.v1.json");
    public static string LocalConfigPath { get; } = Path.Combine(Paths.Data, "config.v1.yml");

    public static bool TryGetConfigItemNoType(string key, [NotNullWhen(true)] out ConfigItem? item)
        => _Items.TryGetValue(key, out item);

    public static bool TryGetConfigItem<TValue>(string key, out ConfigItem<TValue>? item)
    {
        if (!_isConfigItemsInitialized) throw new InvalidOperationException("Not initialized");
        var result = TryGetConfigItemNoType(key, out var value);
        item = result ? (value as ConfigItem<TValue>) : null;
        return result;
    }

    public static ConfigItem<TValue> GetConfigItem<TValue>(string key)
    {
        var result = TryGetConfigItem<TValue>(key, out var item);
        if (!result) throw new KeyNotFoundException($"Config key not found: '{key}'");
        return item ?? throw new InvalidCastException($"Type of '{key}' is incompatible with {typeof(TValue).FullName}");
    }

    public static void RegisterObserver(IConfigScope scope, ConfigObserver observer)
    {
        var itemKeys = scope.CheckScope(KeySet);
        foreach (var key in itemKeys)
        {
            var item = _Items[key];
            item.Observe(observer);
        }
    }

    #region Providers

    private static ConfigStorage? _sharedConfigProvider;
    private static ConfigStorage? _sharedEncryptedConfigProvider;
    private static ConfigStorage? _localConfigProvider;
    private static ConfigStorage? _instanceConfigProvider;

    public static IConfigProvider GetProvider(ConfigSource source)
    {
        if (!_isProvidersInitialized) throw new InvalidOperationException("Not initialized");
        return source switch
        {
            ConfigSource.Shared => _sharedConfigProvider!,
            ConfigSource.SharedEncrypt => _sharedEncryptedConfigProvider!,
            ConfigSource.Local => _localConfigProvider!,
            ConfigSource.GameInstance => _instanceConfigProvider!,
            _ => throw new ArgumentException($"Invalid source: {source}")
        };
    }

    private static void _InitializeProviders()
    {
        Action[] inits = [
            () =>
            {
                if (!File.Exists(SharedConfigPath))
                {
                    string[] oldPaths = [
                        Path.Combine(Paths.OldSharedData, "Config.json"),
                        Path.Combine(Paths.SharedData, "config.json")
                    ];
                    _TryMigrate(SharedConfigPath, oldPaths.Select(path =>
                        new ConfigMigration { From = path, To = SharedConfigPath, OnMigration = SharedJsonMigration }));
                }
                var fileProvider = new JsonFileProvider(SharedConfigPath);
                var storage = new FileConfigStorage(fileProvider);
                _sharedConfigProvider = storage;
                _sharedEncryptedConfigProvider = new EncryptedFileConfigStorage(storage);
            },
            () =>
            {
                if (!File.Exists(LocalConfigPath)) _TryMigrate(LocalConfigPath, [
                    new ConfigMigration
                    {
                        From = Path.Combine(Paths.Data, "setup.ini"),
                        To = LocalConfigPath,
                        OnMigration = CatIniMigration
                    }
                ]);
                var fileProvider = new YamlFileProvider(LocalConfigPath);
                _localConfigProvider = new FileConfigStorage(fileProvider);
            },
            () =>
            {
                _instanceConfigProvider = new DynamicCacheConfigStorage
                {
                    StorageFactory = argument =>
                    {
                        ArgumentNullException.ThrowIfNull(argument);
                        var dir = Path.GetFullPath(argument.ToString()!);
                        var configPath = Path.Combine(dir, "PCL", "config.v1.yml");
                        if (!File.Exists(dir)) _TryMigrate(dir, [
                            new ConfigMigration
                            {
                                From = Path.Combine(dir, "PCL", "setup.ini"),
                                To = configPath,
                                OnMigration = CatIniMigration
                            }
                        ]);
                        var fileProvider = new YamlFileProvider(configPath);
                        var storage = new FileConfigStorage(fileProvider);
                        return storage;
                    }
                };
            }
        ];
        try { Task.WaitAll(inits.Select(Task.Run).ToArray()); }
        catch (AggregateException ex) { throw ex.GetBaseException(); }

        return;

        void SharedJsonMigration(string from, string to)
        {
            File.Copy(from, to);
        }

        void CatIniMigration(string from, string to)
        {
            var lines = File.ReadAllLines(from);
            var yamlProvider = new YamlFileProvider(to);
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                var kv = line.Split(':', 2);
                if (kv.Length != 2) continue;
                yamlProvider.Set(kv[0], kv[1]);
            }
            yamlProvider.Sync();
        }
    }

    private static void _TryMigrate(string target, IEnumerable<ConfigMigration> migrations)
    {
        try
        {
            var result = ConfigMigration.Migrate(target, migrations);
            if (!result) { }
        }
        catch (Exception ex)
        {
        }
    }

    #endregion

    #region Lifecycle & Initialization

    public static bool IsInitialized { get; private set; } = false;

    private static bool _isProvidersInitialized = false;
    private static bool _isConfigItemsInitialized = false;

    public static void Start()
    {
        if (IsInitialized) return;
        var timer = new Stopwatch();
        timer.Start();
        try
        {
            _InitializeConfigItems();
            _isConfigItemsInitialized = true;
            _InitializeProviders();
            _isProvidersInitialized = true;
            foreach (var (_, item) in _Items)
            {
                item.TriggerEvent(ConfigEvent.Init, null, true, true);
            }
            IsInitialized = true;
        }
        catch (Exception ex)
        {
            var currentSection = _isConfigItemsInitialized ? "OBSERVER" : _isProvidersInitialized ? "CONFIG_ITEM" : "PROVIDER";
            var msg = $"配置初始化失败，当前位于 {currentSection} 阶段。";
            if (ex is ConfigFileInitException e)
            {
                var filePath = e.Path;
                var backupPath = e.Path + ".failbackup";
                var bakPath = e.Path + ".bak";
                File.Move(filePath, backupPath, true);
                if (File.Exists(bakPath)) File.Copy(bakPath, filePath, true);
            }
            throw new InvalidOperationException(msg, ex);
        }
        timer.Stop();
    }

    public static void Stop()
    {
        _sharedConfigProvider?.Stop();
        _localConfigProvider?.Stop();
        _instanceConfigProvider?.Stop();
    }

    #endregion

    private static void _InitializeConfigItems() { }
}