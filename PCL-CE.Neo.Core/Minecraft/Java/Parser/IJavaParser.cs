namespace PCL_CE.Neo.Core.Minecraft.Java.Parser;

public interface IJavaParser
{
    JavaInstallation? Parse(string javaExePath);
}