using Microsoft.Extensions.DependencyInjection;

namespace FloppyBot.Commands.Core.Cooldown;

public static class DiSetup
{
    public static IServiceCollection AddCooldown(this IServiceCollection services)
    {
        return services.AddSingleton<ICooldownService, CooldownService>();
    }
}
