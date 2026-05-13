namespace PCL_CE.Neo.Core.Abstractions;

public interface ILifecycleBridge
{
    event Action<string>? ServiceStarting;
    event Action<string>? ServiceStarted;
    event Action<string>? ServiceStopping;
    event Action<string>? ServiceStopped;
    event Action<string, Exception>? ServiceException;

    void Initialize();
    Task StartAsync();
    Task StopAsync();
    void RequestShutdown();
    void OnException(Exception ex);
}
