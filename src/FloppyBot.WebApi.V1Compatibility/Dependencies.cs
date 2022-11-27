using FloppyBot.Commands.Aux.Quotes;
using FloppyBot.Commands.Aux.Twitch;
using FloppyBot.Commands.Custom.Execution;
using FloppyBot.Commands.Registry;
using FloppyBot.WebApi.V1Compatibility.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FloppyBot.WebApi.V1Compatibility;

public static class Dependencies
{
    public static IServiceCollection AddV1Compatibility(this IServiceCollection services)
    {
        ShoutoutCommand.RegisterDependencies(services);
        QuoteCommands.RegisterDependencies(services);
        CustomCommandHost.WebDiSetup(services);
        return services
            .AddDistributedCommandRegistry()
            .AddSingleton<V1CommandConverter>();
    }
}
