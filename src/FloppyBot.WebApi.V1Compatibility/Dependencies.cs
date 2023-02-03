using FloppyBot.Commands.Aux.Quotes;
using FloppyBot.Commands.Aux.Twitch;
using FloppyBot.Commands.Core.Config;
using FloppyBot.Commands.Custom.Execution;
using FloppyBot.Commands.Registry;
using FloppyBot.FileStorage;
using FloppyBot.WebApi.V1Compatibility.Controllers;
using FloppyBot.WebApi.V1Compatibility.DataImport;
using FloppyBot.WebApi.V1Compatibility.Hubs;
using FloppyBot.WebApi.V1Compatibility.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace FloppyBot.WebApi.V1Compatibility;

public static class Dependencies
{
    public static IServiceCollection AddV1Compatibility(this IServiceCollection services)
    {
        ShoutoutCommand.RegisterDependencies(services);
        ShoutoutCommand.SetupTimerMessageDbDependencies(services);
        QuoteCommands.RegisterDependencies(services);
        CustomCommandHost.WebDiSetup(services);
        return services
            .AddFileStorage()
            .AddDistributedCommandRegistry()
            .AddSingleton<V1CommandConverter>()
            .AddSingleton<SoundCommandInvocationCollector>()
            .AddTransient<V1DataImportService>()
            .AddSingleton<ICommandConfigurationService, CommandConfigurationService>();
    }

    public static HubEndpointConventionBuilder MapV1SignalRHub(this IEndpointRouteBuilder endpoints)
    {
        // Start collector
        endpoints.ServiceProvider.GetRequiredService<SoundCommandInvocationCollector>();
        return endpoints.MapHub<V1SoundCommandHub>($"/{V1Config.ROUTE_BASE}hub/sound-command");
    }
}
