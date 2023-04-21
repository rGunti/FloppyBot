using System.Collections.Immutable;
using System.Reflection;
using FloppyBot.Commands.Core.Attributes.Dependencies;
using FloppyBot.Commands.Core.Attributes.Guards;
using FloppyBot.Commands.Core.Config;
using FloppyBot.Commands.Core.Cooldown;
using FloppyBot.Commands.Core.Entities;
using FloppyBot.Commands.Core.Executor;
using FloppyBot.Commands.Core.Guard;
using FloppyBot.Commands.Core.Internal;
using FloppyBot.Commands.Core.ListSupplier;
using FloppyBot.Commands.Core.Metadata;
using FloppyBot.Commands.Core.Replier;
using FloppyBot.Commands.Core.Spawner;
using FloppyBot.Commands.Core.Support;
using Microsoft.Extensions.DependencyInjection;

namespace FloppyBot.Commands.Core.Scan;

public static class CommandScannerExtensions
{
    private static readonly ICommandScanner CommandScanner = new CommandScanner();

    public static IServiceCollection ScanAndAddCommandDependencies(this IServiceCollection services)
    {
        return services
            .ScanAndAddCommandHandlers()
            .ScanAndAddVariableCommandHandlers()
            .AddSingleton<CommandListSupplierAggregator>()
            .AddCommandListSupplier<BuiltInCommandListSupplier>()
            .AddSingleton<ICommandSpawner, CommandSpawner>()
            .AddSingleton<ICommandExecutor, CommandExecutor>()
            .AddSingleton<IMetadataExtractor, MetadataExtractor>()
            .AddSingleton<IMessageReplier, MessageReplier>()
            .AddGuards()
            .AddTasks()
            .AddCooldown()
            .AddCommandConfiguration();
    }

    public static IEnumerable<CommandInfo> ScanTypeForCommandHandlers<T>(
        this ICommandScanner scanner
    )
    {
        return scanner.ScanTypeForCommandHandlers(typeof(T));
    }

    private static IServiceCollection ScanAndAddCommandHandlers(this IServiceCollection services)
    {
        IImmutableDictionary<string, CommandInfo> handlers =
            CommandScanner.ScanForCommandHandlers();

        foreach (var type in handlers.Select(i => i.Value.ImplementingType).Distinct())
        {
            services.AddScoped(type).ScanForCommandDependencies(type);
        }

        return services.AddSingleton(handlers);
    }

    private static IServiceCollection ScanAndAddVariableCommandHandlers(
        this IServiceCollection services
    )
    {
        ImmutableList<VariableCommandInfo> handlers = CommandScanner
            .ScanForVariableCommandHandlers()
            .ToImmutableList();

        foreach (Type type in handlers.Select(i => i.ImplementingType).Distinct())
        {
            services.AddScoped(type).ScanForCommandDependencies(type);
        }

        return services.AddSingleton<IImmutableList<VariableCommandInfo>>(handlers);
    }

    private static IServiceCollection AddGuards(this IServiceCollection services)
    {
        return services
            .AddGuardRegistry()
            .AddGuard<PrivilegeGuard, PrivilegeGuardAttribute>()
            .AddGuard<SourceInterfaceGuard, SourceInterfaceGuardAttribute>();
    }

    public static IServiceCollection AddCommandListSupplier<T>(this IServiceCollection services)
        where T : class, ICommandListSupplier
    {
        return services.AddSingleton<ICommandListSupplier, T>();
    }

    private static void ScanForCommandDependencies(
        this IServiceCollection serviceCollection,
        Type type
    )
    {
        foreach (
            var diRegistrationMethod in type.GetMethods(BindingFlags.Static | BindingFlags.Public)
                .Where(m => m.IsStatic && m.HasCustomAttribute<DependencyRegistrationAttribute>())
        )
        {
            var parameters = diRegistrationMethod.GetParameters();
            if (parameters.Length == 1 && parameters[0].ParameterType == typeof(IServiceCollection))
            {
                diRegistrationMethod.Invoke(null, new[] { serviceCollection });
            }
            else
            {
                throw new InvalidOperationException(
                    $"A method marked with [{nameof(DependencyRegistrationAttribute)}] must be static and "
                        + $"must only contain one parameter of type {typeof(IServiceCollection)}."
                );
            }
        }
    }
}
