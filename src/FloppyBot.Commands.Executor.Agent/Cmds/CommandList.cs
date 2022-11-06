using FloppyBot.Base.TextFormatting;
using FloppyBot.Chat;
using FloppyBot.Commands.Core.Attributes;
using FloppyBot.Commands.Core.Attributes.Metadata;
using FloppyBot.Commands.Core.Executor;
using FloppyBot.Commands.Executor.Agent.Utils;
using FloppyBot.Commands.Parser.Entities;

namespace FloppyBot.Commands.Executor.Agent.Cmds;

[CommandHost]
[CommandCategory("Diagnostics")]
// ReSharper disable once UnusedType.Global
public class CommandList
{
    private const string REPLY_DEFAULT = "These are the commands I know: {CommandList}";
    private const string REPLY_MD = "These are the commands I know: `{CommandList}`";

    private const string COMMAND_LIST_DELIMITER_DEFAULT = ", ";
    private const string COMMAND_LIST_DELIMITER_MD = "`, `";

    private readonly ICommandExecutor _commandExecutor;

    public CommandList(ICommandExecutor commandExecutor)
    {
        _commandExecutor = commandExecutor;
    }

    [Command("commands")]
    [CommandDescription("Returns a list of all available commands")]
    // ReSharper disable once UnusedMember.Global
    public string? ListCommands(CommandInstruction instruction)
    {
        var commands = _commandExecutor.KnownCommands
            .Select(c => c.Names[0])
            .Distinct()
            .OrderBy(i => i);

        var commandList = string.Join(
            instruction.DetermineMessageTemplate(
                ChatInterfaceFeatures.MarkdownText,
                COMMAND_LIST_DELIMITER_MD,
                COMMAND_LIST_DELIMITER_DEFAULT),
            commands);

        return instruction.DetermineMessageTemplate(
                ChatInterfaceFeatures.MarkdownText,
                REPLY_MD,
                REPLY_DEFAULT)
            .Format(new
            {
                CommandList = commandList
            });
    }
}
