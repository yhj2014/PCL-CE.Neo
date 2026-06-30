using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PCL_CE.Neo.Core.Utils;

public static class JsonHelper
{
    private static readonly JsonSerializerOptions _defaultOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = false,
        AllowTrailingCommas = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private static readonly JsonSerializerOptions _indentedOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        AllowTrailingCommas = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static string Serialize(object? value, bool indented = false)
    {
        var options = indented ? _indentedOptions : _defaultOptions;
        return JsonSerializer.Serialize(value, options);
    }

    public static string Serialize<T>(T? value, bool indented = false)
    {
        var options = indented ? _indentedOptions : _defaultOptions;
        return JsonSerializer.Serialize(value, options);
    }

    public static string Serialize(object? value, JsonSerializerOptions options)
    {
        return JsonSerializer.Serialize(value, options ?? _defaultOptions);
    }

    public static T? Deserialize<T>(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return default;

        return JsonSerializer.Deserialize<T>(json, _defaultOptions);
    }

    public static object? Deserialize(string? json, Type type)
    {
        if (string.IsNullOrWhiteSpace(json))
            return default;

        return JsonSerializer.Deserialize(json, type, _defaultOptions);
    }

    public static T? Deserialize<T>(string? json, JsonSerializerOptions options)
    {
        if (string.IsNullOrWhiteSpace(json))
            return default;

        return JsonSerializer.Deserialize<T>(json, options ?? _defaultOptions);
    }

    public static async Task<string> SerializeAsync(object? value, bool indented = false)
    {
        using var stream = new System.IO.MemoryStream();
        var options = indented ? _indentedOptions : _defaultOptions;
        await JsonSerializer.SerializeAsync(stream, value, options);
        stream.Position = 0;
        using var reader = new System.IO.StreamReader(stream);
        return await reader.ReadToEndAsync();
    }

    public static async Task<T?> DeserializeAsync<T>(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return default;

        using var stream = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(json));
        return await JsonSerializer.DeserializeAsync<T>(stream, _defaultOptions);
    }

    public static async Task<object?> DeserializeAsync(string? json, Type type)
    {
        if (string.IsNullOrWhiteSpace(json))
            return default;

        using var stream = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(json));
        return await JsonSerializer.DeserializeAsync(stream, type, _defaultOptions);
    }

    public static bool IsValidJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return false;

        try
        {
            JsonDocument.Parse(json);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    public static string FormatJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return json ?? string.Empty;

        try
        {
            using var doc = JsonDocument.Parse(json);
            return JsonSerializer.Serialize(doc, _indentedOptions);
        }
        catch (JsonException)
        {
            return json;
        }
    }

    public static Dictionary<string, object?> ParseToDictionary(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return new Dictionary<string, object?>();

        var result = Deserialize<Dictionary<string, object?>>(json);
        return result ?? new Dictionary<string, object?>();
    }

    public static T DeepClone<T>(T? obj)
    {
        if (obj == null)
            return default!;

        var json = Serialize(obj);
        return Deserialize<T>(json)!;
    }
}