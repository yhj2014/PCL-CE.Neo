using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using PCL_CE.Neo.Core.Logging;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;

namespace PCL_CE.Neo.Core.Configuration.Storage;

public class YamlFileProvider : CommonFileProvider, IEnumerableKeyProvider
{
    private readonly YamlMappingNode _rootNode;

    private static readonly IDeserializer _Deserializer = new DeserializerBuilder()
        .IgnoreUnmatchedProperties().WithEnforceRequiredMembers().Build();

    private static readonly ISerializer _Serializer = new SerializerBuilder()
        .DisableAliases().Build();

    private static YamlMappingNode? _LoadFile(string path)
    {
        if (!File.Exists(path)) return null;
        
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var reader = new StreamReader(stream, Encoding.UTF8);
        
        try
        {
            var yaml = new YamlStream();
            yaml.Load(reader);
            if (yaml.Documents.Count == 0) return [];
            
            var rootNode = yaml.Documents[0].RootNode;
            return rootNode as YamlMappingNode ?? 
                throw new ConfigFileInitException(path, $"Invalid root node type: {rootNode.NodeType}");
        }
        catch (Exception ex)
        {
            if (ex is ConfigFileInitException) throw;
            throw new ConfigFileInitException(path, "Failed to load YAML content", ex);
        }
    }

    public YamlFileProvider(string path) : base(path)
    {
        var rootNode = _LoadFile(path);
        if (rootNode != null)
        {
            _rootNode = rootNode;
            return;
        }

        try
        {
            var jsonPath = Path.Combine(Path.GetDirectoryName(path) ?? string.Empty, 
                Path.GetFileNameWithoutExtension(path) + ".json");
            
            if (File.Exists(jsonPath))
            {
                using var jsonStream = new FileStream(jsonPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var yamlStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
                JsonToYamlConverter.Convert(jsonStream, yamlStream);
                _rootNode = _LoadFile(path)!;
                return;
            }
        }
        catch (Exception ex)
        {
            LogWrapper.Warn(ex, "转换失败，已忽略");
        }

        _rootNode = [];
    }

    public override T Get<T>(string key)
    {
        var result = _rootNode.Children[key];
        var parser = result.ConvertToEventStream().GetParser();
        
        try
        {
            return _Deserializer.Deserialize<T>(parser);
        }
        catch (Exception)
        {
            var type = typeof(T);
            var graphStr = result.ToString();
            var fallback = (type == typeof(bool))
                ? (T)(object)(graphStr.ToLowerInvariant() is "true" or "1")
                : (T)(object)graphStr;
            
            Set(key, fallback);
            return fallback;
        }
    }

    public override void Set<T>(string key, T value)
    {
        var emitter = new YamlNodeEmitter();
        _Serializer.Serialize(emitter, value);
        _rootNode.Children[key] = emitter.SingleRootNode;
    }

    public override bool Exists(string key)
    {
        return _rootNode.Children.ContainsKey(key);
    }

    public override void Delete(string key)
    {
        _rootNode.Children.Remove(key);
    }

    protected override void WriteToStream(Stream stream)
    {
        var writer = new StreamWriter(stream, Encoding.UTF8);
        _Serializer.Serialize(writer, _rootNode);
        writer.Flush();
    }

    public IEnumerable<string> Keys => _rootNode.Select(pair => pair.Key.ToString());
}