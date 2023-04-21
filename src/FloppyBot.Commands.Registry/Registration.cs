using Microsoft.Extensions.DependencyInjection;

namespace FloppyBot.Commands.Registry;

public static class Registration
{
    public static IServiceCollection AddDistributedCommandRegistry(this IServiceCollection services)
    {
        return services.AddSingleton<IDistributedCommandRegistry, DistributedCommandRegistry>();
    }
}
