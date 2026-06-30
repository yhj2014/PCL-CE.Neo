using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;

namespace PCL_CE.Neo.Core.Configuration.Storage;

public abstract class ConfigStorage : IConfigProvider
{
    protected abstract bool OnAccess<TKey, TValue>(
        StorageAction action,
        ref TKey key,
        [NotNullWhen(true)] ref TValue value,
        object? argument);

    protected virtual void OnStop() { }

    public void Stop() => OnStop();

    public bool Access<TKey, TValue>(
        StorageAction action,
        ref TKey key,
        [NotNullWhen(true)] ref TValue value,
        object? argument)
    {
        var hasOutput = false;
        try
        {
            hasOutput = OnAccess(action, ref key, ref value, argument);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Config Storage Error: {ex.Message}", ex);
        }
        return hasOutput;
    }

    public bool GetValue<T>(string key, [NotNullWhen(true)] out T? value, object? argument = null)
    {
        var keyRef = key;
        T? valueRef = default;
        var hasValue = Access(StorageAction.Get, ref keyRef, ref valueRef, argument);
        value = valueRef;
        return hasValue;
    }

    public void SetValue<T>(string key, T value, object? argument = null)
    {
        var keyRef = key;
        var valueRef = value;
        Access(StorageAction.Set, ref keyRef, ref valueRef, argument);
    }

    public void Delete(string key, object? argument = null)
    {
        var keyRef = key;
        object? valueRef = null;
        Access(StorageAction.Delete, ref keyRef, ref valueRef, argument);
    }

    public bool Exists(string key, object? argument = null)
    {
        var keyRef = key;
        var resultRef = false;
        return Access(StorageAction.Exists, ref keyRef, ref resultRef, argument) && resultRef;
    }

    public override string ToString() => $"{GetType().Name}@{GetHashCode()}";
}