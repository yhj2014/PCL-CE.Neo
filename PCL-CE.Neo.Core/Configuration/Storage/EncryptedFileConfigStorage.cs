using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using PCL_CE.Neo.Core.Logging;
using PCL_CE.Neo.Core.Utils.Secret;

namespace PCL_CE.Neo.Core.Configuration.Storage;

public class EncryptedFileConfigStorage(ConfigStorage source) : ConfigStorage
{
    public ConfigStorage Source { get; } = source;

    private static readonly JsonSerializerOptions _SerializerOptions = new()
    {
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        AllowOutOfOrderMetadataProperties = true,
    };

    public override bool GetValue<T>(string key, out T value, object? argument = null)
    {
        value = default!;
        
        try
        {
            string? raw = null;
            var hasOutput = Source.GetValue(key, out raw, argument);
            if (!hasOutput) return false;

            var decrypted = EncryptHelper.SecretDecrypt(raw);
            var type = typeof(T);
            
            if (type == typeof(bool))
                value = (T)(object)(decrypted.ToLowerInvariant() is "true" or "1");
            else if (type == typeof(string))
                value = (T)(object)decrypted;
            else
                value = JsonSerializer.Deserialize<T>(decrypted, _SerializerOptions) ?? 
                    throw new NullReferenceException("Decryption produced a null reference");
            
            return true;
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, "Config", "无法处理解密");
            return false;
        }
    }

    public override void SetValue<T>(string key, T value, object? argument = null)
    {
        try
        {
            var type = typeof(T);
            string strValue;
            
            if (type == typeof(string))
                strValue = value?.ToString() ?? string.Empty;
            else
                strValue = JsonSerializer.Serialize(value, _SerializerOptions);

            strValue = EncryptHelper.SecretEncrypt(strValue);
            Source.SetValue(key, strValue, argument);
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, "Config", "无法处理加密");
        }
    }

    public override bool Exists(string key, object? argument = null)
    {
        return Source.Exists(key, argument);
    }

    public override void Delete(string key, object? argument = null)
    {
        Source.Delete(key, argument);
    }
}