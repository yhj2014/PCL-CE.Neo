using System;
using Uno.WinUI.Runtime.Skia.X11;

public class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("Testing X11 type resolution");
        var host = new X11ApplicationHost(() => null);
        Console.WriteLine($"Created host: {host != null}");
    }
}
