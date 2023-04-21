using Microsoft.Extensions.DependencyInjection;

namespace FloppyBot.Commands.Core.Config;

public static class DiSetup
{
    public static IServiceCollection AddCommandConfiguration(this IServiceCollection services)
    {
        return services.AddSingleton<ICommandConfigurationService, CommandConfigurationService>();
    }
}
