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
        typeof(ChatMessage),
        // Short Version: only return message
        typeof(string)
    }.ToImmutableHashSet();

    public IImmutableDictionary<string, CommandInfo> ScanForCommandHandlers()
    {
        return ScanTypesForCommandHandlers(AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes()))
            .SelectMany(commandInfo => commandInfo.Names
                .Select(commandName => new
                {
                    Name = commandName,
                    CommandInfo = commandInfo
                }))
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
                nameof(type));
        }

        return type
            .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
            .Where(method => method.HasCustomAttribute<CommandAttribute>())
            .Inspect(method =>
            {
                if (!IsCommandSignatureValid(method))
                {
                    throw new InvalidCommandSignatureException(method);
                }
            })
            .Select(method => new CommandInfo(
                method.GetCustomAttribute<CommandAttribute>()!.Names.ToImmutableListWithValueSemantics(),
                method));
    }

    private static bool IsCommandSignatureValid(MethodInfo method)
    {
        return AllowedReturnTypes.Contains(method.ReturnType);
    }
}
