using FloppyBot.Chat.Entities.Identifiers;
using FloppyBot.Commands.Core.Config;
using FloppyBot.Commands.Core.Entities;
using FloppyBot.Commands.Parser.Entities;

namespace FloppyBot.Commands.Core.Support.PreExecution;

[TaskOrder(ORDER)]
public class DisabledCommandTask : IPreExecutionTask
{
    private const int ORDER = 5;

    private readonly ICommandConfigurationService _commandConfigurationService;

    public DisabledCommandTask(ICommandConfigurationService commandConfigurationService)
    {
        _commandConfigurationService = commandConfigurationService;
    }

    public bool ExecutePre(CommandInfo info, CommandInstruction instruction)
    {
        return _commandConfigurationService
            .GetCommandConfiguration(instruction.Context!.SourceMessage.Identifier.GetChannel(),
                info.CommandId)
            .Select(config => !config.Disabled)
            .FirstOrDefault(true);
    }
}
