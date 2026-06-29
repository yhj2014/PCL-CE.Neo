using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PCL_CE.Neo.Core.Serialization;

public static class JsonHelper
{
    private static readonly JsonSerializerOptions DefaultOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReferenceHandler = ReferenceHandler.Preserve,
        WriteIndented = true
    };

    public static string Serialize<T>(T value, bool pretty = false)
    {
        try
        {
            var options = pretty 
                ? new JsonSerializerOptions(DefaultOptions) { WriteIndented = true }
                : new JsonSerializerOptions(DefaultOptions) { WriteIndented = false };

            return JsonSerializer.Serialize(value, options);
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, "Failed to serialize object");
            throw;
        }
    }

    public static T? Deserialize<T>(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<T>(json, DefaultOptions);
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, "Failed to deserialize JSON");
            return default;
        }
    }

    public static object? Deserialize(string json, Type type)
    {
        try
        {
            return JsonSerializer.Deserialize(json, type, DefaultOptions);
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, "Failed to deserialize JSON");
            return null;
        }
    }

    public static byte[] SerializeToUtf8Bytes<T>(T value)
    {
        try
        {
            return JsonSerializer.SerializeToUtf8Bytes(value, DefaultOptions);
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, "Failed to serialize to UTF8 bytes");
            throw;
        }
    }

    public static T? DeserializeFromUtf8Bytes<T>(byte[] bytes)
    {
        try
        {
            return JsonSerializer.Deserialize<T>(bytes, DefaultOptions);
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, "Failed to deserialize from UTF8 bytes");
            return default;
        }
    }

    public static string FormatJson(string json)
    {
        try
        {
            var element = JsonSerializer.Deserialize<JsonElement>(json);
            return JsonSerializer.Serialize(element, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            LogWrapper.Warn(ex, "Failed to format JSON, returning original");
            return json;
        }
    }

    public static bool TryDeserialize<T>(string json, out T? result)
    {
        try
        {
            result = JsonSerializer.Deserialize<T>(json, DefaultOptions);
            return true;
        }
        catch
        {
            result = default;
            return false;
        }
    }
}