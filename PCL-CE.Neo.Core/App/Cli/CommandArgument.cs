namespace PCL_CE.Neo.Core.App.Cli;

public abstract class CommandArgument
{
    public required string Key { get; init; }
    public abstract ArgumentValueKind ValueKind { get; }
    public required string ValueText { get; init; }
    public abstract bool TryCastValue<T>(out T value);
    public abstract T CastValue<T>();
}