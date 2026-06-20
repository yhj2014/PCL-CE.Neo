using System;

namespace PCL_CE.Neo.Core.App.Essentials;

public class RpcPropertyOperationFailedException : Exception;

public class RpcProperty
{
    public delegate void GetValueDelegate(out string? outValue);

    public event GetValueDelegate GetValue;

    public delegate void SetValueDelegate(string? value, ref bool success);

    public event SetValueDelegate? SetValue;

    public readonly string Name;
    public readonly bool Settable = true;

    public string? Value
    {
        get
        {
            GetValue.Invoke(out var value);
            return value;
        }
        set
        {
            var success = true;
            SetValue?.Invoke(value, ref success);
            if (!success)
                throw new RpcPropertyOperationFailedException();
        }
    }

    public RpcProperty(string name, Func<string?> onGetValue, Action<string?>? onSetValue = null, bool settable = false)
    {
        Name = name;
        GetValue += (out outValue) => { outValue = onGetValue(); };
        if (onSetValue != null)
        {
            SetValue += (value, ref _) => { onSetValue(value); };
        }
        else if (!settable)
        {
            Settable = false;
            SetValue += (_, ref success) => { success = false; };
        }
    }
}