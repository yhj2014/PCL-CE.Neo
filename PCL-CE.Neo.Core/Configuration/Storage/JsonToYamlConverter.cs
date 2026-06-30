using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;

namespace PCL_CE.Neo.Core.Configuration.Storage;

public static class JsonToYamlConverter
{
    public static void Convert(Stream jsonStream, Stream yamlStream)
    {
        var jsonNode = JsonNode.Parse(jsonStream);
        var yamlNode = ConvertNode(jsonNode!);
        var serializer = new SerializerBuilder().DisableAliases().Build();
        using var writer = new StreamWriter(yamlStream);
        serializer.Serialize(writer, yamlNode);
        writer.Flush();
    }

    private static YamlNode ConvertNode(JsonNode jsonNode)
    {
        return jsonNode switch
        {
            JsonObject jsonObject => ConvertObject(jsonObject),
            JsonArray jsonArray => ConvertArray(jsonArray),
            JsonValue jsonValue => ConvertValue(jsonValue),
            _ => new YamlScalarNode(jsonNode.ToString())
        };
    }

    private static YamlMappingNode ConvertObject(JsonObject jsonObject)
    {
        var mapping = new YamlMappingNode();
        foreach (var (key, value) in jsonObject)
        {
            mapping.Add(key, value != null ? ConvertNode(value) : new YamlScalarNode(string.Empty));
        }
        return mapping;
    }

    private static YamlSequenceNode ConvertArray(JsonArray jsonArray)
    {
        var sequence = new YamlSequenceNode();
        foreach (var item in jsonArray)
        {
            sequence.Add(item != null ? ConvertNode(item) : new YamlScalarNode(string.Empty));
        }
        return sequence;
    }

    private static YamlNode ConvertValue(JsonValue jsonValue)
    {
        if (jsonValue.TryGetValue(out bool boolValue))
            return new YamlScalarNode(boolValue);
        if (jsonValue.TryGetValue(out int intValue))
            return new YamlScalarNode(intValue);
        if (jsonValue.TryGetValue(out double doubleValue))
            return new YamlScalarNode(doubleValue);
        return new YamlScalarNode(jsonValue.ToString());
    }
}