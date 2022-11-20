using FloppyBot.Base.TextFormatting;
using FloppyBot.Chat.Entities;
using FloppyBot.Commands.Core.Attributes;
using FloppyBot.Commands.Core.Attributes.Args;
using FloppyBot.Commands.Core.Attributes.Guards;
using FloppyBot.Commands.Core.Attributes.Metadata;
using FloppyBot.Commands.Core.Entities;
using FloppyBot.Commands.Custom.Storage;

namespace FloppyBot.Commands.Custom.Execution.Administration;

[CommandHost]
[CommandCategory("Custom Commands")]
[PrivilegeGuard(PrivilegeLevel.Moderator)]
public class CustomCommandAdministrationCommands
{
    private const string REPLY_CREATE_SUCCESS = "New command {CommandName} created";

    private const string REPLY_CREATE_FAILED =
        "Could not create command with name {CommandName}. There is already a command with this name.";

    private const string REPLY_DELETE_SUCCESS = "Command {CommandName} successfully deleted";
    private const string REPLY_DELETE_FAILED = "Could nto delete command with name {CommandName}";

    private readonly ICustomCommandService _commandService;

    public CustomCommandAdministrationCommands(ICustomCommandService commandService)
    {
        _commandService = commandService;
    }

    [Command("newcmd")]
    [CommandDescription("Creates a new custom text command")]
    [CommandSyntax("<Command Name> <Reply Text>")]
    public CommandResult CreateCommand(
        [SourceChannel] string sourceChannel,
        [ArgumentIndex(0)]
        string commandName,
        [ArgumentRange(1)]
        string commandResponse)
    {
        bool created = _commandService.CreateSimpleCommand(sourceChannel, commandName, commandResponse);
        var formatParams = new
        {
            CommandName = commandName
        };
        return created
            ? new CommandResult(CommandOutcome.Success, REPLY_CREATE_SUCCESS.Format(formatParams))
            : new CommandResult(CommandOutcome.Failed, REPLY_CREATE_FAILED.Format(formatParams));
    }

    [Command("deletecmd")]
    [CommandDescription("Deletes a custom text command")]
    [CommandSyntax("<Command Name>")]
    public CommandResult DeleteCommand(
        [SourceChannel] string sourceChannel,
        [ArgumentIndex(0)]
        string commandName)
    {
        bool deleted = _commandService.DeleteCommand(sourceChannel, commandName);
        var formatParams = new
        {
            CommandName = commandName
        };
        return deleted
            ? new CommandResult(CommandOutcome.Success, REPLY_DELETE_SUCCESS.Format(formatParams))
            : new CommandResult(CommandOutcome.Failed, REPLY_DELETE_FAILED.Format(formatParams));
    }
}
