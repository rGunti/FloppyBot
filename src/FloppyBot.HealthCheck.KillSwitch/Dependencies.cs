using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FloppyBot.HealthCheck.KillSwitch;

public static class Dependencies
{
    public static IServiceCollection AddKillSwitch(this IServiceCollection services)
    {
        return services
            .AddSingleton<KillSwitchReceiver>();
    }

    public static IServiceCollection AddKillSwitchTrigger(this IServiceCollection services)
    {
        return services
            .AddScoped<IKillSwitchTrigger, KillSwitchTriggerSender>();
    }

    public static THost ArmKillSwitch<THost>(this THost host) where THost : IHost
    {
        host.Services.GetRequiredService<KillSwitchReceiver>();
        return host;
    }
}
