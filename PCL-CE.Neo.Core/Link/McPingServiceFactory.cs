using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace PCL_CE.Neo.Core.Link;

public class McPingServiceFactory : IMcPingServiceFactory
{
    private readonly ILogger<McPingServiceFactory> _logger;
    private readonly IServiceProvider? _serviceProvider;

    public McPingServiceFactory()
    {
        _logger = LoggerFactory.Create(b => b.AddConsole()).CreateLogger<McPingServiceFactory>();
    }

    public McPingServiceFactory(ILogger<McPingServiceFactory> logger)
    {
        _logger = logger;
    }

    public McPingServiceFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _logger = serviceProvider.GetRequiredService<ILogger<McPingServiceFactory>>();
    }

    public IMcPingService CreateService(string host, int port, int timeout = 5000)
    {
        _logger.LogDebug("Creating modern McPingService for {Host}:{Port}", host, port);
        
        if (_serviceProvider != null)
        {
            return ActivatorUtilities.CreateInstance<McPingService>(_serviceProvider, host, port, timeout);
        }
        
        return new McPingService(host, port, timeout);
    }

    public IMcPingService CreateLegacyService(string host, int port, int timeout = 5000)
    {
        _logger.LogDebug("Creating legacy McPingService for {Host}:{Port}", host, port);
        
        if (_serviceProvider != null)
        {
            return ActivatorUtilities.CreateInstance<LegacyMcPingService>(_serviceProvider, host, port, timeout);
        }
        
        return new LegacyMcPingService(host, port, timeout);
    }
}
