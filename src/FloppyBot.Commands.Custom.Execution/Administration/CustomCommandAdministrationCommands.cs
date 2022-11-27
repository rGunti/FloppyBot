using FloppyBot.Base.Extensions;
using FloppyBot.Base.TextFormatting;
using FloppyBot.Chat.Entities;
using FloppyBot.Commands.Core.Attributes;
using FloppyBot.Commands.Core.Attributes.Args;
using FloppyBot.Commands.Core.Attributes.Guards;
using FloppyBot.Commands.Core.Attributes.Metadata;
using FloppyBot.Commands.Core.Entities;
using FloppyBot.Commands.Custom.Storage;

namespace FloppyBot.Commands.Custom.Execution.Administration;

internal record CustomCommandFormattingParams(
    string CommandName,
    int Counter = int.MinValue);

[CommandHost]
[CommandCategory("Custom Commands")]
[PrivilegeGuard(PrivilegeLevel.Moderator)]
public class CustomCommandAdministrationCommands
{
    public const string REPLY_CREATE_SUCCESS = "New command {CommandName} created";

    public const string REPLY_CREATE_FAILED =
        "Could not create command with name {CommandName}. There is already a command with this name.";

    public const string REPLY_DELETE_SUCCESS = "Command {CommandName} successfully deleted";
    public const string REPLY_DELETE_FAILED = "Could nto delete command with name {CommandName}";

    public const string REPLY_COMMAND_NOT_FOUND = "Command {CommandName} doesn't exist";
    public const string REPLY_COUNTER_SET = "Counter for {CommandName} is now set to {Counter}";

    public const string REPLY_COUNTER_PARAMS_UNKNOWN =
        "Provide an absolute value, a relative value (prefixed with + or -) or the word \"clear\" to reset it to 0";

    private const string CMD_COUNTER_CLEAR = "clear";

    private readonly ICustomCommandService _commandService;
    private readonly ICounterStorageService _counterStorageService;

    public CustomCommandAdministrationCommands(
        ICustomCommandService commandService,
        ICounterStorageService counterStorageService)
    {
        _commandService = commandService;
        _counterStorageService = counterStorageService;
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
        var formatParams = new CustomCommandFormattingParams(commandName);
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
        var formatParams = new CustomCommandFormattingParams(commandName);
        return deleted
            ? new CommandResult(CommandOutcome.Success, REPLY_DELETE_SUCCESS.Format(formatParams))
            : new CommandResult(CommandOutcome.Failed, REPLY_DELETE_FAILED.Format(formatParams));
    }

    [Command("counter", "count")]
    [PrimaryCommandName("counter")]
    [CommandDescription("Sets the counter for a custom command")]
    [CommandSyntax("<Command Name> <Operation>|<Value>")]
    public CommandResult SetCounter(
        [SourceChannel] string sourceChannel,
        [ArgumentIndex(0, stopIfMissing: true)]
        string commandName,
        [ArgumentIndex(1, stopIfMissing: true)]
        string operationOrValue)
    {
        string? commandId = _commandService.GetCommand(sourceChannel, commandName)
            .Wrap()
            .Select(c => c.Id)
            .FirstOrDefault();
        var formatParams = new CustomCommandFormattingParams(commandName);
        if (commandId == null)
        {
            return new CommandResult(CommandOutcome.Failed, REPLY_COMMAND_NOT_FOUND.Format(formatParams));
        }

        switch (operationOrValue)
        {
            case CMD_COUNTER_CLEAR:
                _counterStorageService.Set(commandId, 0);
                return CreateCounterResult(formatParams with { Counter = 0 });
        }

        if ((operationOrValue.StartsWith("+") || operationOrValue.StartsWith("-"))
            && int.TryParse(operationOrValue, out int incrementValue))
        {
            // Relative
            int incrementedValue = _counterStorageService.Increase(commandId, incrementValue);
            return CreateCounterResult(formatParams with
            {
                Counter = incrementedValue
            });
        }

        if (int.TryParse(operationOrValue, out int newValue))
        {
            // Absolute
            _counterStorageService.Set(commandId, newValue);
            return CreateCounterResult(formatParams with
            {
                Counter = newValue
            });
        }

        return new CommandResult(CommandOutcome.Failed, REPLY_COUNTER_PARAMS_UNKNOWN);
    }

    private static CommandResult CreateCounterResult(CustomCommandFormattingParams formattingParams)
    {
        return new CommandResult(CommandOutcome.Success, REPLY_COUNTER_SET.Format(formattingParams));
    }
}
