using FloppyBot.Commands.Aux.Quotes;
using FloppyBot.Commands.Aux.Twitch;
using FloppyBot.Commands.Core.Config;
using FloppyBot.Commands.Custom.Execution;
using FloppyBot.FileStorage;
using FloppyBot.WebApi.V2.Hubs;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace FloppyBot.WebApi.V2;

public static class V2Dependencies
{
    public static IServiceCollection AddV2Dependencies(this IServiceCollection services)
    {
        ShoutoutCommand.RegisterDependencies(services);
        ShoutoutCommand.SetupTimerMessageDbDependencies(services);
        QuoteCommands.RegisterDependencies(services);
        CustomCommandHost.WebDiSetup(services);
        return services
            .AddFileStorage()
            .AddSingleton<ICommandConfigurationService, CommandConfigurationService>();
    }

    public static HubEndpointConventionBuilder MapV2SignalRHub(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapHub<StreamSourceHub>($"/hub/stream-source");
    }
}
