using FloppyBot.Base.Auditing.Abstraction;
using FloppyBot.Base.Auditing.Storage;
using FloppyBot.Commands.Aux.Quotes;
using FloppyBot.Commands.Aux.Twitch;
using FloppyBot.Commands.Custom.Execution;
using FloppyBot.Commands.Registry;
using FloppyBot.FileStorage;
using Microsoft.Extensions.DependencyInjection;

namespace FloppyBot.WebApi.V2;

public static class Registration
{
    public static IServiceCollection AddV2WebApi(this IServiceCollection services)
    {
        ShoutoutCommand.RegisterDependencies(services);
        ShoutoutCommand.SetupTimerMessageDbDependencies(services);
        QuoteCommands.RegisterDependencies(services);
        CustomCommandHost.WebDiSetup(services);
        return services
            .AddFileStorage()
            .AddDistributedCommandRegistry()
            .AddAuditor<StorageAuditor>();
    }
}
