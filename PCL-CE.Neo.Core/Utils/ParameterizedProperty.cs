namespace PCL_CE.Neo.Core.Utils;

public class ParameterizedProperty<TValue, TParam>
{
    private readonly Func<TParam, TValue> _getter;
    private readonly Action<TParam, TValue>? _setter;

    public ParameterizedProperty(Func<TParam, TValue> getter)
    {
        _getter = getter ?? throw new ArgumentNullException(nameof(getter));
    }

    public ParameterizedProperty(Func<TParam, TValue> getter, Action<TParam, TValue> setter)
    {
        _getter = getter ?? throw new ArgumentNullException(nameof(getter));
        _setter = setter ?? throw new ArgumentNullException(nameof(setter));
    }

    public TValue this[TParam param]
    {
        get => _getter(param);
        set
        {
            if (_setter == null)
                throw new InvalidOperationException("Property is read-only");
            _setter(param, value);
        }
    }

    public bool CanWrite => _setter != null;
}