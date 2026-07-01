using System.Text;
using System.Text.Json;

namespace PCL_CE.Neo.Core.Utils.Serialization;

public class JsonSerializer : ISerializer
{
    private readonly JsonSerializerOptions _options;

    public JsonSerializer()
    {
        _options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true,
            AllowTrailingCommas = true,
            IgnoreNullValues = true
        };
    }

    public JsonSerializer(JsonSerializerOptions options)
    {
        _options = options;
    }

    public string Serialize<T>(T obj)
    {
        return System.Text.Json.JsonSerializer.Serialize(obj, _options);
    }

    public T? Deserialize<T>(string data)
    {
        return System.Text.Json.JsonSerializer.Deserialize<T>(data, _options);
    }

    public byte[] SerializeToBytes<T>(T obj)
    {
        string json = Serialize(obj);
        return Encoding.UTF8.GetBytes(json);
    }

    public T? DeserializeFromBytes<T>(byte[] data)
    {
        string json = Encoding.UTF8.GetString(data);
        return Deserialize<T>(json);
    }
}