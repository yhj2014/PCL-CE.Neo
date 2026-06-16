using System;
using PCL_CE.Neo.Core.Logging;

namespace PCL_CE.Neo.Core.Configuration.Storage;

public class FileConfigStorage : ConfigStorage
{
    private readonly IKeyValueFileProvider _fileProvider;

    public FileConfigStorage(IKeyValueFileProvider fileProvider)
    {
        _fileProvider = fileProvider;
    }

    public override bool GetValue<T>(string key, out T value, object? argument = null)
    {
        if (_fileProvider.TryGet(key, out var objValue))
        {
            try
            {
                if (objValue is T typedValue)
                {
                    value = typedValue;
                    return true;
                }
                value = ConvertTo<T>(objValue);
                return true;
            }
            catch (Exception ex)
            {
                LogWrapper.Warn(ex, "FileConfigStorage", $"Failed to convert value for key: {key}");
            }
        }
        value = default!;
        return false;
    }

    public override void SetValue<T>(string key, T value, object? argument = null)
    {
        _fileProvider.Set(key, value);
        _fileProvider.Sync();
    }

    public override bool Exists(string key, object? argument = null)
    {
        return _fileProvider.Exists(key);
    }

    public override void Delete(string key, object? argument = null)
    {
        _fileProvider.Delete(key);
        _fileProvider.Sync();
    }

    private static T ConvertTo<T>(object value)
    {
        if (typeof(T).IsEnum)
            return (T)Enum.Parse(typeof(T), value.ToString() ?? string.Empty);
        
        return (T)System.Convert.ChangeType(value, typeof(T));
    }
}