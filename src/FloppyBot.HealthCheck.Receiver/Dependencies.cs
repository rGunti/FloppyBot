using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FloppyBot.HealthCheck.Receiver;

public static class Dependencies
{
    public static IServiceCollection AddHealthCheckReceiver(this IServiceCollection services)
    {
        return services.AddSingleton<IHealthCheckReceiver, HealthCheckReceiver>();
    }

    public static THost StartHealthCheckReceiver<THost>(this THost host)
        where THost : IHost
    {
        // Start receiving health checks
        host.Services.GetRequiredService<IHealthCheckReceiver>();
        return host;
    }
}
