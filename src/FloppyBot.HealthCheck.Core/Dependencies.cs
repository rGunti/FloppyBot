using FloppyBot.Base.Clock;
using FloppyBot.Base.Cron;
using Microsoft.Extensions.DependencyInjection;

namespace FloppyBot.HealthCheck.Core;

public static class Dependencies
{
    public static IServiceCollection AddHealthCheck(this IServiceCollection services)
    {
        return services
            .AddSingleton<IHealthCheckProducer, HealthCheckProducer>()
            .AddSingleton<ITimeProvider, RealTimeProvider>()
            .AddCronJob<HealthCheckProducerCronJob>();
    }
}
