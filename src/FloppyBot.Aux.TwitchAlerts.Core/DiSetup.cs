using FloppyBot.Aux.TwitchAlerts.Core.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace FloppyBot.Aux.TwitchAlerts.Core;

public static class DiSetup
{
    public static IServiceCollection AddTwitchAlertCore(this IServiceCollection services)
    {
        return services.AddSingleton<TwitchAlertListener>();
    }

    public static IServiceCollection AddTwitchAlertService(this IServiceCollection services)
    {
        return services.AddTransient<ITwitchAlertService, TwitchAlertService>();
    }
}
