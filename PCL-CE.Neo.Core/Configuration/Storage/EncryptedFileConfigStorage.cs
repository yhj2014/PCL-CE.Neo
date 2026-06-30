using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using PCL_CE.Neo.Core.Utils.Secret;

namespace PCL_CE.Neo.Core.Configuration.Storage;

public class EncryptedFileConfigStorage : ConfigStorage
{
    private readonly ILogger<EncryptedFileConfigStorage> _logger;
    private readonly ConfigStorage _innerStorage;

    public EncryptedFileConfigStorage(ConfigStorage innerStorage) : this(innerStorage, Microsoft.Extensions.Logging.Abstractions.NullLogger<EncryptedFileConfigStorage>.Instance) { }

    public EncryptedFileConfigStorage(ConfigStorage innerStorage, ILogger<EncryptedFileConfigStorage> logger)
    {
        _innerStorage = innerStorage;
        _logger = logger;
    }

    protected override void OnStop()
    {
        _innerStorage.Stop();
    }

    protected override bool OnAccess<TKey, TValue>(
        StorageAction action,
        ref TKey key,
        [NotNullWhen(true)] ref TValue value,
        object? argument)
    {
        if (key is not string strKey) throw new NotSupportedException($"Key '{key}' is not supported");
        switch (action)
        {
            case StorageAction.Get:
                if (!_innerStorage.Exists(strKey)) return false;
                var encryptedValue = _innerStorage.GetValue<string>(strKey, out var encrypted) ? encrypted : null;
                if (encryptedValue == null) return false;
                try
                {
                    var decrypted = EncryptHelper.DecryptString(encryptedValue);
                    value = System.Text.Json.JsonSerializer.Deserialize<TValue>(decrypted) ?? default!;
                    return true;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "解密配置值失败: {Key}", strKey);
                    return false;
                }
            case StorageAction.Exists:
                return _innerStorage.Exists(strKey);
            case StorageAction.Set:
                try
                {
                    var json = System.Text.Json.JsonSerializer.Serialize(value);
                    var encrypted = EncryptHelper.EncryptString(json);
                    _innerStorage.SetValue(strKey, encrypted);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "加密配置值失败: {Key}", strKey);
                }
                return false;
            case StorageAction.Delete:
                _innerStorage.Delete(strKey);
                return false;
            default: throw new InvalidOperationException($"Invalid storage action: {action}");
        }
    }

    public override string ToString() => $"Encrypted({_innerStorage})";
}