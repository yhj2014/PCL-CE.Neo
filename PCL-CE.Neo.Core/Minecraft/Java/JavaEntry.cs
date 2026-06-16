namespace PCL_CE.Neo.Core.Minecraft.Java;

public enum JavaSource
{
    AutoScanned,
    ManualAdded
}

public class JavaEntry
{
    public required JavaInstallation Installation { get; init; }
    public bool IsEnabled { get; set; }
    public JavaSource Source { get; set; }

    public override string ToString() => Installation.ToString();
}