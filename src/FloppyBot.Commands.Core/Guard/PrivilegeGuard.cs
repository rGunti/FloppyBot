using FloppyBot.Chat.Entities;
using FloppyBot.Chat.Entities.Identifiers;
using FloppyBot.Commands.Core.Attributes.Guards;
using FloppyBot.Commands.Core.Config;
using FloppyBot.Commands.Core.Entities;
using FloppyBot.Commands.Parser.Entities;

namespace FloppyBot.Commands.Core.Guard;

public class PrivilegeGuard : BaseGuard<PrivilegeGuardAttribute>
{
    private readonly ICommandConfigurationService _commandConfigurationService;

    public PrivilegeGuard(ICommandConfigurationService commandConfigurationService)
    {
        _commandConfigurationService = commandConfigurationService;
    }

    public override bool CanExecute(
        CommandInstruction instruction,
        CommandInfo command,
        PrivilegeGuardAttribute settings
    )
    {
        ChatMessage sourceMessage = instruction.Context!.SourceMessage;
        PrivilegeLevel minRequiredLevel =
            _commandConfigurationService
                .GetCommandConfiguration(sourceMessage.Identifier.GetChannel(), command.CommandId)
                .Select(config => config.RequiredPrivilegeLevel)
                .FirstOrDefault()
            ?? settings.MinLevel;

        return instruction.Context!.SourceMessage.Author.PrivilegeLevel >= minRequiredLevel;
    }
}
