namespace PCL_CE.Neo.Core.App.Cli;

public class TextArgument : CommandArgument
{
    public override ArgumentValueKind ValueKind => ArgumentValueKind.Text;

    public override bool TryCastValue<T>(out T value)
    {
        if (typeof(T) == typeof(string))
        {
            value = (T)(object)CastValue<string>();
            return true;
        }
        value = default!;
        return false;
    }

    public override T CastValue<T>()
    {
        if (typeof(T) == typeof(string))
            return (T)(object)ValueText;
        throw new InvalidCastException($"Cannot cast text to {typeof(T).Name}");
    }
}