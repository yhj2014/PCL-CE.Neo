namespace PCL_CE.Neo.Core.Minecraft;

public sealed class JavaEntry
{
    public required Java.JavaInstallation Installation { get; init; }
    public bool IsEnabled { get; set; } = true;
    public Java.JavaSource Source { get; set; } = Java.JavaSource.AutoScanned;

    public override string ToString() =>
        $"{(IsEnabled ? "[✓]" : "[ ]")} {Installation}";
}