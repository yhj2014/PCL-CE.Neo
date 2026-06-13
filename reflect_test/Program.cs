using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

namespace ReflectTest;

class Program
{
    static void Main(string[] args)
    {
        var skiaPath = "/root/.nuget/packages/uno.winui.runtime.skia/5.5.0-dev.465/lib/net9.0/Uno.UI.Runtime.Skia.dll";
        var asm = AssemblyLoadContext.Default.LoadFromAssemblyPath(skiaPath);

        var builderType = asm.GetType("Uno.UI.Runtime.Skia.SkiaHostBuilder");
        if (builderType != null)
        {
            Console.WriteLine("=== SkiaHostBuilder Methods ===");
            foreach (var m in builderType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static))
            {
                var pars = string.Join(", ", m.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}"));
                Console.WriteLine($"  {m.ReturnType.Name} {m.Name}({pars})");
            }
        }

        var extType = asm.GetType("Uno.UI.Runtime.Skia.SkiaHostBuilderExtensions");
        if (extType != null)
        {
            Console.WriteLine("=== SkiaHostBuilderExtensions Methods ===");
            foreach (var m in extType.GetMethods(BindingFlags.Public | BindingFlags.Static))
            {
                var pars = string.Join(", ", m.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}"));
                Console.WriteLine($"  {m.ReturnType.Name} {m.Name}({pars})");
            }
        }

        Console.WriteLine("=== GtkHost Constructors ===");
        var gtkPath = "/root/.nuget/packages/uno.winui.runtime.skia.gtk/5.5.0-dev.465/lib/net9.0/Uno.UI.Runtime.Skia.Gtk.dll";
        var gtkAsm = AssemblyLoadContext.Default.LoadFromAssemblyPath(gtkPath);
        var gtkType = gtkAsm.GetType("Uno.UI.Runtime.Skia.Gtk.GtkHost");
        if (gtkType != null)
        {
            foreach (var c in gtkType.GetConstructors(BindingFlags.Public | BindingFlags.Instance))
            {
                var pars = string.Join(", ", c.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}"));
                Console.WriteLine($"  {gtkType.Name}({pars})");
            }
            foreach (var m in gtkType.GetMethods(BindingFlags.Public | BindingFlags.Instance))
            {
                var pars = string.Join(", ", m.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}"));
                Console.WriteLine($"  {m.ReturnType.Name} {m.Name}({pars})");
            }
        }

        Console.WriteLine("=== MacSkiaHost Constructors ===");
        var macPath = "/root/.nuget/packages/uno.winui.runtime.skia.macos/5.5.0-dev.465/lib/net9.0/Uno.UI.Runtime.Skia.MacOS.dll";
        var macAsm = AssemblyLoadContext.Default.LoadFromAssemblyPath(macPath);
        var macType = macAsm.GetType("Uno.UI.Runtime.Skia.MacOS.MacSkiaHost");
        if (macType != null)
        {
            foreach (var c in macType.GetConstructors(BindingFlags.Public | BindingFlags.Instance))
            {
                var pars = string.Join(", ", c.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}"));
                Console.WriteLine($"  {macType.Name}({pars})");
            }
            foreach (var m in macType.GetMethods(BindingFlags.Public | BindingFlags.Instance))
            {
                var pars = string.Join(", ", m.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}"));
                Console.WriteLine($"  {m.ReturnType.Name} {m.Name}({pars})");
            }
        }
    }
}
