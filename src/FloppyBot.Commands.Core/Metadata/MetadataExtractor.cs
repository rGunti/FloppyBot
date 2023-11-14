using System.Collections.Immutable;
using System.Reflection;
using FloppyBot.Base.Extensions;
using FloppyBot.Chat.Entities;
using FloppyBot.Commands.Core.Attributes.Guards;
using FloppyBot.Commands.Core.Attributes.Metadata;
using FloppyBot.Commands.Core.Entities;

namespace FloppyBot.Commands.Core.Metadata;

public class MetadataExtractor : IMetadataExtractor
{
    public CommandMetadata ExtractMetadataFromCommand(CommandInfo commandInfo)
    {
        var classAttributes = commandInfo
            .ImplementingType
            .GetCustomAttributes<CommandMetadataAttribute>()
            .GroupBy(a => a.Type)
            .ToImmutableDictionary(g => g.Key, g => g.ToImmutableList());
        var methodAttributes = commandInfo
            .HandlerMethod
            .GetCustomAttributes<CommandMetadataAttribute>()
            .GroupBy(a => a.Type)
            .ToImmutableDictionary(g => g.Key, g => g.ToImmutableList());

        var dict = new Dictionary<string, string>();
        foreach (var (type, values) in classAttributes)
        {
            dict[type] = values.Select(v => v.Value).Join("\n\n");
        }

        foreach (var (type, values) in methodAttributes)
        {
            dict[type] = values.Select(v => v.Value).Join("\n\n");
        }

        ExtractAdditionalMetadata(commandInfo, dict);

        return new CommandMetadata(dict);
    }

    private void ExtractAdditionalMetadata(
        CommandInfo commandInfo,
        Dictionary<string, string> existingMetadata
    )
    {
        // If no minimum privilege has been defined, check if a guard exists and extract it from there
        if (!existingMetadata.ContainsKey(CommandMetadataTypes.MIN_PRIVILEGE))
        {
            var privilegeLevel = TryExtractRequiredPrivilegeLevel(commandInfo);
            if (privilegeLevel != PrivilegeLevel.Unknown)
            {
                existingMetadata[CommandMetadataTypes.MIN_PRIVILEGE] = privilegeLevel.ToString()!;
            }
        }

        if (!existingMetadata.ContainsKey(CommandMetadataTypes.INTERFACES))
        {
            var interfaceLimits = TryExtractInterfaceLimit(commandInfo);
            if (interfaceLimits.Any())
            {
                existingMetadata[CommandMetadataTypes.INTERFACES] = string.Join(
                    ",",
                    interfaceLimits
                );
            }
        }
    }

    private PrivilegeLevel TryExtractRequiredPrivilegeLevel(CommandInfo commandInfo)
    {
        var methodLevel = commandInfo
            .HandlerMethod
            .GetCustomAttributes<PrivilegeGuardAttribute>()
            .Select(g => g.MinLevel)
            .FirstOrDefault();
        if (methodLevel == PrivilegeLevel.Unknown)
        {
            return commandInfo
                .ImplementingType
                .GetCustomAttributes<PrivilegeGuardAttribute>()
                .Select(g => g.MinLevel)
                .FirstOrDefault();
        }

        return methodLevel;
    }

    private string[] TryExtractInterfaceLimit(CommandInfo commandInfo)
    {
        var methodLevel = commandInfo
            .HandlerMethod
            .GetCustomAttributes<SourceInterfaceGuardAttribute>()
            .Select(g => g.AllowedMessageInterfaces)
            .FirstOrDefault();
        if (methodLevel == null)
        {
            return commandInfo
                    .ImplementingType
                    .GetCustomAttributes<SourceInterfaceGuardAttribute>()
                    .Select(g => g.AllowedMessageInterfaces)
                    .FirstOrDefault()
                    ?.ToArray() ?? Array.Empty<string>();
        }

        return methodLevel.ToArray();
    }
}
