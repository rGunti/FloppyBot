using System.Collections.Immutable;
using FloppyBot.Chat.Entities;
using FloppyBot.Chat.Entities.Identifiers;
using FloppyBot.Commands.Core.ListSupplier;
using FloppyBot.Commands.Custom.Storage;
using FloppyBot.Commands.Parser.Entities;

namespace FloppyBot.Commands.Custom.Execution;

public class CustomCommandListSupplier : ICommandListSupplier
{
    private readonly ICustomCommandService _commandService;

    public CustomCommandListSupplier(ICustomCommandService commandService)
    {
        _commandService = commandService;
    }

    public IEnumerable<string> GetCommandList(CommandInstruction commandInstruction)
    {
        ChannelIdentifier channelId =
            commandInstruction.Context!.SourceMessage.Identifier.GetChannel();
        PrivilegeLevel privilegeLevel = commandInstruction
            .Context!
            .SourceMessage
            .Author
            .PrivilegeLevel;

        return _commandService
            .GetCommandsOfChannel(channelId)
            .Where(c => c.Limitations.MinLevel <= privilegeLevel)
            .Select(c => c.Name)
            .ToImmutableList();
    }
}
