using PCL_CE.Neo.Core.Lifecycle;
using PCL_CE.Neo.Core.Logging;
using PCL_CE.Neo.Core.Minecraft.Java.Scanner;

namespace PCL_CE.Neo.Core.Minecraft.Java;

public sealed class JavaService : ServiceBase
{
    private const string ModuleName = "JavaService";
    private JavaManager? _javaManager;

    public JavaManager JavaManager => _javaManager ?? throw new InvalidOperationException("JavaService 尚未启动");

    public override string Identifier => "java";
    public override string Name => "Java 管理";

    public JavaService(IServiceProvider services) : base(services)
    {
    }

    public override async Task StartAsync()
    {
        if (_javaManager is not null) return;

        LogWrapper.Info(ModuleName, "Initializing Java Manager...");

        _javaManager = new JavaManager(
            Services.GetRequiredService<IJavaParser>(),
            Services.GetServices<IJavaScanner>().ToArray());

        _javaManager.ReadConfig(string.Empty);

        LogWrapper.Info(ModuleName, "Lookup for local Java...");
        await _javaManager.ScanJavaAsync();

        var javaList = _javaManager.GetSortedJavaList();
        if (javaList.Count > 0)
        {
            var logInfo = string.Join("\n\t", javaList.Select(j => $"{j.Installation.Version} ({j.Installation.Brand}) - {j.Installation.JavaExePath}"));
            LogWrapper.Info(ModuleName, $"Finished to scan java: \n\t{logInfo}");
        }
        else
        {
            LogWrapper.Warn(ModuleName, "No Java installations found");
        }
    }

    public override Task StopAsync()
    {
        if (_javaManager is null) return Task.CompletedTask;

        try
        {
            _javaManager.SaveConfig(string.Empty);
            LogWrapper.Info(ModuleName, "Java configuration saved");
        }
        catch (Exception ex)
        {
            LogWrapper.Error(ex, ModuleName, "Failed to save Java configuration");
        }

        _javaManager = null;
        return Task.CompletedTask;
    }
}