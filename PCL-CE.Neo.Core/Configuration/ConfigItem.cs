using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace PCL_CE.Neo.Core.Configuration;

public class ConfigItem<TValue> : IConfigScope, ConfigItem
{
    public string Key { get; }
    public ConfigSource Source { get; }
    public Type Type => typeof(TValue);

    private Func<TValue>? _defaultValueConstructor;
    private TValue? _defaultValue;
    private bool _defaultValueHasSet;

    public ConfigItem(string key, Func<TValue> defaultValue, ConfigSource source)
    {
        Key = key;
        Source = source;
        _defaultValueConstructor = defaultValue;
    }

    public ConfigItem(string key, TValue defaultValue, ConfigSource source)
        : this(key, () => defaultValue, source) { }

    private TValue _GetDefaultValue()
    {
        if (_defaultValueHasSet) return _defaultValue!;
        _defaultValue = _defaultValueConstructor!();
        _defaultValueHasSet = true;
        _defaultValueConstructor = null;
        return _defaultValue;
    }

    public TValue DefaultValue => _GetDefaultValue();
    public object DefaultValueNoType => DefaultValue ?? default!;

    public IEnumerable<string> CheckScope(IReadOnlySet<string> keys) => keys.Contains(Key) ? [Key] : [];

    private IConfigProvider _Provider => field ??= ConfigService.GetProvider(Source);

    private ConfigValueCache<TValue> _valueCache = new();

    public bool EnableCache { get; set; } = true;

    private bool _ProcessNewCache(TValue newCache, object? argument, bool force = false)
    {
        if (!EnableCache) return true;
        if (!force)
        {
            var existsOld = _valueCache.TryRead(out var oldCache, argument);
            if (existsOld && EqualityComparer<TValue>.Default.Equals(oldCache, newCache)) return false;
        }
        if (newCache is INotifyPropertyChanged reactive)
            reactive.PropertyChanged += (_, _) => OnContentChanged();
        else if (newCache is INotifyCollectionChanged reactiveCollection)
            reactiveCollection.CollectionChanged += (_, _) => OnContentChanged();
        _valueCache.Write(newCache, argument);
        return true;

        void OnContentChanged() => SetValue(newCache, argument, bypassCache: true);
    }

    public TValue GetValue(object? argument = null)
    {
        TValue? value = default;
        var exists = EnableCache && _valueCache.TryRead(out value, argument);
        var newValue = false;
        if (!exists)
        {
            newValue = true;
            exists = _Provider.GetValue(Key, out value, argument);
        }
        var e = _TriggerEvent(ConfigEvent.Get, argument, value, true);
        if (e != null)
        {
            if (e.Cancelled) return DefaultValue;
            if (e.NewValueReplacement != null) return (TValue)e.NewValueReplacement;
        }
        if (!exists) value = DefaultValue;
        if (newValue) _ProcessNewCache(value!, argument);
        return value!;
    }

    public object GetValueNoType(object? argument = null) => GetValue(argument) ?? default!;

    public bool SetValue(TValue value, object? argument = null, bool forceNewValue = false, bool bypassCache = false)
    {
        var e = _TriggerEvent(ConfigEvent.Set, argument, value, isPreview: true);
        if (e != null)
        {
            if (e.Cancelled) return false;
            if (e.NewValueReplacement != null) value = (TValue)e.NewValueReplacement;
        }
        if (bypassCache || _ProcessNewCache(value, argument, forceNewValue))
            _Provider.SetValue(Key, value, argument);
        _TriggerEvent(ConfigEvent.Set, argument, value, e: e, isPreview: false);
        return true;
    }

    public bool SetValueNoType(object value, object? argument = null)
    {
        try
        {
            return SetValue((TValue)value, argument);
        }
        catch (InvalidCastException)
        {
            throw new InvalidCastException($"Value convert failed (required: {Type.FullName}, provided: {value.GetType().FullName})");
        }
    }

    public bool SetDefaultValue(object? argument = null, bool? forceNewValue = null)
    {
        return SetValue(DefaultValue, argument, forceNewValue ?? IsDefault(argument));
    }

    public bool Reset(object? argument = null)
    {
        var e = _TriggerEvent(ConfigEvent.Reset, argument, null, isPreview: true);
        if (e is { Cancelled: true }) return false;
        _Provider.Delete(Key, argument);
        if (EnableCache) _valueCache.Invalidate(argument);
        _TriggerEvent(ConfigEvent.Reset, argument, null, isPreview: false);
        return true;
    }

    public bool IsDefault(object? argument = null)
    {
        var result = !_Provider.Exists(Key, argument);
        var e = _TriggerEvent(ConfigEvent.CheckDefault, argument, result);
        if (e is { NewValueReplacement: not null }) result = (bool)e.NewValueReplacement;
        return result;
    }

    private readonly HashSet<ConfigObserver> _observers = [];
    private readonly HashSet<ConfigObserver> _previewObservers = [];

    public void Observe(ConfigObserver observer)
    {
        if (observer.IsPreview) _previewObservers.Add(observer);
        else _observers.Add(observer);
    }

    public bool Unobserve(ConfigObserver observer)
        => observer.IsPreview ? _previewObservers.Remove(observer) : _observers.Remove(observer);

    private object? _GetValueOrNull(object? argument)
    {
        var exists = _Provider.GetValue<TValue>(Key, out var value, argument);
        return exists ? value : null;
    }

    public ConfigEventArgs? TriggerEvent(
        ConfigEvent trigger, object? argument,
        bool bypassOldValue = false, bool fillNewValue = false)
    {
        return _TriggerEvent(trigger, argument, null, bypassOldValue, fillNewValue);
    }

    private ConfigEventArgs? _TriggerEvent(
        ConfigEvent trigger, object? argument, object? newValue,
        bool bypassOldValue = false, bool fillNewValue = false,
        ConfigEventArgs? e = null, bool? isPreview = null)
    {
        var replaceNewValue = false;
        foreach (var observer in (
            from observer in (isPreview is { } p ? (p ? _previewObservers : _observers) : _previewObservers.Concat(_observers))
            let logic = (int)observer.Event & (int)trigger
            where logic > 0
            select observer))
        {
            if (e == null)
            {
                if (isPreview == false && !bypassOldValue) bypassOldValue = true;
                var currentValue = (fillNewValue || !bypassOldValue) ? _GetValueOrNull(argument) : null;
                if (newValue == null && fillNewValue) newValue = currentValue ?? DefaultValue;
                e = new ConfigEventArgs(this, trigger, argument, bypassOldValue ? null : currentValue, newValue);
            }
            observer.Handler(e);
            if (observer.IsPreview)
            {
                if (e.NewValueReplacement != null) replaceNewValue = true;
                if (e.Cancelled) return e;
            }
            else if (!replaceNewValue && e.NewValueReplacement != null) e.NewValueReplacement = null;
        }
        if (e is { Cancelled: true }) e.Cancelled = false;
        return e;
    }
}

public interface ConfigItem
{
    string Key { get; }
    ConfigSource Source { get; }
    Type Type { get; }
    void Observe(ConfigObserver observer);
    bool Unobserve(ConfigObserver observer);
    ConfigEventArgs? TriggerEvent(
        ConfigEvent trigger,
        object? argument,
        bool bypassOldValue = false,
        bool fillNewValue = false
    );
    ConfigObserver Observe(ConfigEvent trigger, ConfigEventHandler handler, bool isPreview = false)
    {
        var observer = new ConfigObserver(trigger, handler, isPreview);
        Observe(observer);
        return observer;
    }
    event ConfigEventHandler Changed
    {
        add => Observe(ConfigEvent.Changed, value);
        remove => throw new NotSupportedException("Please use Observe() and Unobserve()");
    }
    bool Reset(object? argument = null);
    bool IsDefault(object? argument = null);
    bool SetDefaultValue(object? argument = null, bool? forceNewValue = null);
    object GetValueNoType(object? argument = null);
    bool SetValueNoType(object value, object? argument = null);
    object DefaultValueNoType { get; }
    bool EnableCache { get; set; }
}