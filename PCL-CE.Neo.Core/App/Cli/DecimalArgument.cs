using System.Globalization;

namespace PCL_CE.Neo.Core.App.Cli;

public class DecimalArgument : CommandArgument
{
    public override ArgumentValueKind ValueKind => ArgumentValueKind.Decimal;

    public override bool TryCastValue<T>(out T value)
    {
        if (typeof(T) == typeof(decimal))
        {
            value = (T)(object)CastValue<decimal>();
            return true;
        }
        value = default!;
        return false;
    }

    public override T CastValue<T>()
    {
        if (typeof(T) == typeof(decimal))
        {
            if (decimal.TryParse(ValueText, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
                return (T)(object)result;
            return (T)(object)0m;
        }
        throw new InvalidCastException($"Cannot cast decimal to {typeof(T).Name}");
    }
}