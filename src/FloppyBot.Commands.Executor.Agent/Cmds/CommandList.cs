using FloppyBot.Base.TextFormatting;
using FloppyBot.Commands.Core.Attributes;
using FloppyBot.Commands.Core.Executor;

namespace FloppyBot.Commands.Executor.Agent.Cmds;

[CommandHost]
public class CommandList
{
    private const string REPLY = "These are the commands I know: {CommandList}";

    private readonly ICommandExecutor _commandExecutor;

    public CommandList(ICommandExecutor commandExecutor)
    {
        _commandExecutor = commandExecutor;
    }

    [Command("commands")]
    public string? ListCommands()
    {
        return REPLY.Format(new
        {
            CommandList = string.Join(", ", _commandExecutor.KnownCommands
                .SelectMany(c => c.Names))
        });
    }
}
