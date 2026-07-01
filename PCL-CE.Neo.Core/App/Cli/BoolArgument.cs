namespace PCL_CE.Neo.Core.App.Cli;

public class BoolArgument : CommandArgument
{
    public override ArgumentValueKind ValueKind => ArgumentValueKind.Bool;

    public override bool TryCastValue<T>(out T value)
    {
        if (typeof(T) == typeof(bool))
        {
            value = (T)(object)(string.IsNullOrEmpty(ValueText) || 
                string.Equals(ValueText, "true", StringComparison.OrdinalIgnoreCase));
            return true;
        }
        value = default!;
        return false;
    }

    public override T CastValue<T>()
    {
        if (typeof(T) == typeof(bool))
            return (T)(object)(string.IsNullOrEmpty(ValueText) || 
                string.Equals(ValueText, "true", StringComparison.OrdinalIgnoreCase));
        throw new InvalidCastException($"Cannot cast bool to {typeof(T).Name}");
    }
}