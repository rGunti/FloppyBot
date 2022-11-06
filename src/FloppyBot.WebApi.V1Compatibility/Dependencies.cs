using FloppyBot.Commands.Aux.Quotes;
using FloppyBot.Commands.Aux.Twitch;
using Microsoft.Extensions.DependencyInjection;

namespace FloppyBot.WebApi.V1Compatibility;

public static class Dependencies
{
    public static IServiceCollection AddV1Compatibility(this IServiceCollection services)
    {
        ShoutoutCommand.RegisterDependencies(services);
        QuoteCommands.RegisterDependencies(services);
        return services;
    }
}
