using FloppyBot.Base.TextFormatting;
using FloppyBot.Chat;
using FloppyBot.Chat.Entities;
using FloppyBot.Commands.Core.Attributes;
using FloppyBot.Commands.Core.Attributes.Metadata;
using FloppyBot.Commands.Core.ListSupplier;
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

    private readonly CommandListSupplierAggregator _aggregator;

    public CommandList(CommandListSupplierAggregator aggregator)
    {
        _aggregator = aggregator;
    }

    [Command("commands")]
    [CommandDescription("Returns a list of all available commands")]
    [CommandCooldown(PrivilegeLevel.Viewer, 30000)]
    [CommandNoParametersHint]
    // ReSharper disable once UnusedMember.Global
    public string? ListCommands(CommandInstruction instruction)
    {
        var commands = _aggregator.GetCommandList(instruction);

        var commandList = string.Join(
            instruction.DetermineMessageTemplate(
                ChatInterfaceFeatures.MarkdownText,
                COMMAND_LIST_DELIMITER_MD,
                COMMAND_LIST_DELIMITER_DEFAULT
            ),
            commands.Distinct().OrderBy(i => i)
        );

        return instruction
            .DetermineMessageTemplate(ChatInterfaceFeatures.MarkdownText, REPLY_MD, REPLY_DEFAULT)
            .Format(new { CommandList = commandList });
    }
}
