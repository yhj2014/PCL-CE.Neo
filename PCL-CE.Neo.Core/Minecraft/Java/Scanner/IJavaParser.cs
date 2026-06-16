namespace PCL_CE.Neo.Core.Minecraft.Java.Scanner;

public interface IJavaParser
{
    JavaInstallation? Parse(string javaExePath);
}