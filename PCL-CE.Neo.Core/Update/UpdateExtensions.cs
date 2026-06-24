using Microsoft.Extensions.DependencyInjection;
using PCL_CE.Neo.Core.Abstractions;

namespace PCL_CE.Neo.Core.Update;

public static class UpdateExtensions
{
    public static IServiceCollection AddUpdateService(this IServiceCollection services)
    {
        services.AddSingleton<IUpdateService, UpdateService>();
        return services;
    }
}
