using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace PCL_CE.Neo.Core.Configuration.Storage;

public static class JsonToYamlConverter
{
    public static void Convert(Stream jsonInput, Stream yamlOutput, bool leaveOpen = false)
    {
        ArgumentNullException.ThrowIfNull(jsonInput);
        ArgumentNullException.ThrowIfNull(yamlOutput);
        if (!jsonInput.CanRead) throw new ArgumentException("must be readable", nameof(jsonInput));
        if (!yamlOutput.CanWrite) throw new ArgumentException("must be writable", nameof(yamlOutput));

        using var doc = JsonDocument.Parse(jsonInput);
        var obj = _ConvertElement(doc.RootElement);

        var serializer = new SerializerBuilder().Build();
        using var writer = new StreamWriter(yamlOutput, new UTF8Encoding(false), 8192, leaveOpen);
        serializer.Serialize(writer, obj);
        writer.Flush();
    }

    public static async Task ConvertAsync(Stream jsonInput, Stream yamlOutput, bool leaveOpen = false)
    {
        ArgumentNullException.ThrowIfNull(jsonInput);
        ArgumentNullException.ThrowIfNull(yamlOutput);
        if (!jsonInput.CanRead) throw new ArgumentException("jsonInput must be readable", nameof(jsonInput));
        if (!yamlOutput.CanWrite) throw new ArgumentException("yamlOutput must be writable", nameof(yamlOutput));

        using var doc = await JsonDocument.ParseAsync(jsonInput).ConfigureAwait(false);
        var obj = _ConvertElement(doc.RootElement);

        var serializer = new SerializerBuilder().Build();
        await using var streamWriter = new StreamWriter(yamlOutput, new UTF8Encoding(false), 8192, leaveOpen);
        serializer.Serialize(streamWriter, obj);
        await streamWriter.FlushAsync().ConfigureAwait(false);
    }

    private static object? _ConvertElement(JsonElement element)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
            {
                var dict = new Dictionary<string, object?>(StringComparer.Ordinal);
                foreach (var prop in element.EnumerateObject())
                {
                    dict[prop.Name] = _ConvertElement(prop.Value);
                }
                return dict;
            }

            case JsonValueKind.Array:
                return element.EnumerateArray().Select(_ConvertElement).ToList();

            case JsonValueKind.String:
                return element.GetString();

            case JsonValueKind.Number:
            {
                if (element.TryGetInt64(out var l)) return l;

                var raw = element.GetRawText();
                if (decimal.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out var dec)) return dec;

                if (element.TryGetDouble(out var d)) return d;

                return raw;
            }

            case JsonValueKind.True:
                return true;

            case JsonValueKind.False:
                return false;

            case JsonValueKind.Null:
            case JsonValueKind.Undefined:
            default:
                return null;
        }
    }
}