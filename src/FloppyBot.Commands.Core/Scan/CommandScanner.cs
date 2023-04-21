using System.Collections.Immutable;
using System.Reflection;
using FloppyBot.Base.EquatableCollections;
using FloppyBot.Chat.Entities;
using FloppyBot.Commands.Core.Attributes;
using FloppyBot.Commands.Core.Entities;
using FloppyBot.Commands.Core.Exceptions;
using FloppyBot.Commands.Core.Internal;

namespace FloppyBot.Commands.Core.Scan;

public class CommandScanner : ICommandScanner
{
    private static readonly IImmutableSet<Type> AllowedReturnTypes = new[]
    {
        // Default
        typeof(CommandResult),
        typeof(Task<CommandResult>),
        // Deprecated
        typeof(ChatMessage),
        typeof(Task<ChatMessage>),
        // Short Version: only return message
        typeof(string),
        typeof(Task<string>),
    }.ToImmutableHashSet();

    public IImmutableDictionary<string, CommandInfo> ScanForCommandHandlers()
    {
        return IndexCommands(
            ScanTypesForCommandHandlers(
                AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes())
            )
        );
    }

    public IEnumerable<VariableCommandInfo> ScanForVariableCommandHandlers()
    {
        return ScanTypesForVariableCommandHandlers(
            AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes())
        );
    }

    public IImmutableDictionary<string, CommandInfo> IndexCommands(
        IEnumerable<CommandInfo> commands
    )
    {
        return commands
            .SelectMany(
                commandInfo =>
                    commandInfo.Names.Select(
                        commandName => new { Name = commandName, CommandInfo = commandInfo }
                    )
            )
            .ToImmutableDictionary(i => i.Name, i => i.CommandInfo);
    }

    public IEnumerable<CommandInfo> ScanTypesForCommandHandlers(IEnumerable<Type> types)
    {
        return types
            .Where(t => t.HasCustomAttribute<CommandHostAttribute>())
            .Distinct()
            .SelectMany(ScanTypeForCommandHandlers);
    }

    public IEnumerable<CommandInfo> ScanTypeForCommandHandlers(Type type)
    {
        if (!type.HasCustomAttribute<CommandHostAttribute>())
        {
            throw new ArgumentException(
                $"Type {type} does not have {nameof(CommandHostAttribute)} attached and cannot be used to host command handlers",
                nameof(type)
            );
        }

        return type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
            .Where(method => method.HasCustomAttribute<CommandAttribute>())
            .Inspect(method =>
            {
                if (!IsCommandSignatureValid(method))
                {
                    throw new InvalidCommandSignatureException(method);
                }
            })
            .Select(
                method =>
                    new CommandInfo(
                        method
                            .GetCustomAttribute<CommandAttribute>()!
                            .Names.ToImmutableListWithValueSemantics(),
                        method
                    )
            );
    }

    public IEnumerable<VariableCommandInfo> ScanTypesForVariableCommandHandlers(
        IEnumerable<Type> types
    )
    {
        return types
            .Where(t => t.HasCustomAttribute<VariableCommandHostAttribute>())
            .Distinct()
            .SelectMany(ScanTypeForVariableCommandHandlers);
    }

    public IEnumerable<VariableCommandInfo> ScanTypeForVariableCommandHandlers(Type type)
    {
        if (!type.HasCustomAttribute<VariableCommandHostAttribute>())
        {
            throw new ArgumentException(
                $"Type {type} does not have {nameof(VariableCommandHostAttribute)} attached and cannot be used to host variable command handlers",
                nameof(type)
            );
        }

        return type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
            .Where(method => method.HasCustomAttribute<VariableCommandHandlerAttribute>())
            .Inspect(method =>
            {
                if (!IsVariableCommandSignatureValid(method))
                {
                    throw new InvalidCommandSignatureException(method);
                }
            })
            .Select(
                method =>
                    new VariableCommandInfo(
                        method.GetCustomAttribute<VariableCommandHandlerAttribute>()!.Identifier
                            ?? GetDefaultIdentifierForVariableCommandHandler(method),
                        method,
                        method.ReflectedType!.GetMethod(
                            method
                                .GetCustomAttribute<VariableCommandHandlerAttribute>()!
                                .AssertionHandlerName!
                        )!
                    )
            );
    }

    private static bool IsCommandSignatureValid(MethodInfo method)
    {
        return AllowedReturnTypes.Contains(method.ReturnType);
    }

    private static bool IsVariableCommandSignatureValid(MethodInfo method)
    {
        return method.ReturnType == typeof(CommandResult);
    }

    private static string GetDefaultIdentifierForVariableCommandHandler(MemberInfo method)
    {
        return $"{method.DeclaringType!.FullName}.{method.Name}";
    }
}
