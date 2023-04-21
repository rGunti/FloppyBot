using FloppyBot.Commands.Core.Entities;
using FloppyBot.Commands.Parser.Entities;
using Microsoft.Extensions.Logging;

namespace FloppyBot.Commands.Core.Support.Hybrid;

[TaskOrder(int.MinValue)]
public class LogTask : IHybridTask
{
    private readonly ILogger<LogTask> _logger;

    public LogTask(ILogger<LogTask> logger)
    {
        _logger = logger;
    }

    public bool ExecutePost(CommandInfo info, CommandInstruction instruction, CommandResult result)
    {
        _logger.LogDebug(
            "Post-execution tasks for command {Command}: Result was {CommandResult}",
            info,
            result
        );
        return true;
    }

    public bool ExecutePre(CommandInfo info, CommandInstruction instruction)
    {
        _logger.LogDebug("Pre-execution tasks for command {Command}", info);
        return true;
    }
}
