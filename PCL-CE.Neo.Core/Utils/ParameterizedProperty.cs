using System;

namespace PCL_CE.Neo.Core.Utils;

public class ParameterizedProperty<TParam, TValue>
{
    public Func<TParam, TValue> GetValue { private get; init; } = null!;
    public Action<TParam, TValue> SetValue { private get; init; } = null!;

    public TValue this[TParam param]
    {
        get => GetValue.Invoke(param);
        set => SetValue.Invoke(param, value);
    }
}