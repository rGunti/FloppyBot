using System.Collections.Immutable;
using FloppyBot.Commands.Core.Entities;
using FloppyBot.Commands.Core.Executor;
using FloppyBot.Commands.Core.Spawner;
using Microsoft.Extensions.DependencyInjection;

namespace FloppyBot.Commands.Core.Scan;

public static class CommandScannerExtensions
{
    private static readonly ICommandScanner CommandScanner = new CommandScanner();

    public static IServiceCollection ScanAndAddCommandDependencies(
        this IServiceCollection services)
    {
        IImmutableDictionary<string, CommandInfo> handlers = CommandScanner.ScanForCommandHandlers();

        foreach (var type in handlers
                     .Select(i => i.Value.ImplementingType)
                     .Distinct())
        {
            services.AddScoped(type);
        }

        return services
            .AddSingleton(handlers)
            .AddSingleton<ICommandSpawner, CommandSpawner>()
            .AddSingleton<ICommandExecutor, CommandExecutor>();
    }

    public static IEnumerable<CommandInfo> ScanTypeForCommandHandlers<T>(this ICommandScanner scanner)
    {
        return scanner.ScanTypeForCommandHandlers(typeof(T));
    }
}
