using Microsoft.Extensions.DependencyInjection;

namespace FloppyBot.Base.Cron;

public static class Dependencies
{
    public static IServiceCollection AddCronJobSupport(this IServiceCollection services)
    {
        return services.AddSingleton<ICronManager, CronManager>();
    }

    public static IServiceCollection AddCronJob<TCronJob>(this IServiceCollection services)
        where TCronJob : class, ICronJob
    {
        return services.AddSingleton<ICronJob, TCronJob>();
    }
}
