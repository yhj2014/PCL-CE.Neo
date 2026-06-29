using PCL_CE.Neo.Core.Minecraft.Java;

namespace PCL_CE.Neo.Core.Minecraft;

public sealed class JavaEntry
{
    public required JavaInstallation Installation { get; init; }
    public bool IsEnabled { get; set; } = true;
    public JavaSource Source { get; set; } = JavaSource.AutoScanned;

    public override string ToString() =>
        $"{(IsEnabled ? "[✓]" : "[ ]")} {Installation}";
}