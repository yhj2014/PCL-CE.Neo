namespace PCL_CE.Neo.Core.Minecraft.Java;

public record JavaStorageItem
{
    public required string Path { get; init; }
    public bool IsEnable { get; init; }
    public JavaSource? Source { get; init; }
}