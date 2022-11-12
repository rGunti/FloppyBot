using FloppyBot.Base.Clock;
using FloppyBot.Chat.Entities;
using FloppyBot.Chat.Entities.Identifiers;
using FloppyBot.Commands.Core.Cooldown;
using FloppyBot.Commands.Core.Entities;
using FloppyBot.Commands.Parser.Entities;
using Microsoft.Extensions.Logging;

namespace FloppyBot.Commands.Core.Support.Hybrid;

[TaskOrder(ORDER)]
public class CooldownTask : IHybridTask
{
    private const int ORDER = 20;
    private readonly ICooldownService _cooldownService;

    private readonly ILogger<CooldownTask> _logger;
    private readonly ITimeProvider _timeProvider;

    public CooldownTask(
        ILogger<CooldownTask> logger,
        ICooldownService cooldownService,
        ITimeProvider timeProvider)
    {
        _logger = logger;
        _cooldownService = cooldownService;
        _timeProvider = timeProvider;
    }

    public bool ExecutePre(CommandInfo info, CommandInstruction instruction)
    {
        ChatMessage sourceMessage = instruction.Context!.SourceMessage;
        DateTimeOffset lastExecution = _cooldownService.GetLastExecution(
            sourceMessage.Identifier.GetChannel(),
            sourceMessage.Author.Identifier,
            info.CommandId);

        TimeSpan delta = _timeProvider.GetCurrentUtcTime() - lastExecution;

        if (delta < TimeSpan.FromSeconds(5))
        {
            _logger.LogDebug(
                "Last execution did not pass cooldown check, delta was {CooldownDelta}",
                delta);
            return false;
        }

        return true;
    }

    public bool ExecutePost(CommandInfo info, CommandInstruction instruction, CommandResult result)
    {
        if (result.Outcome != CommandOutcome.Failed)
        {
            ChatMessage sourceMessage = instruction.Context!.SourceMessage;
            _cooldownService.StoreExecution(
                sourceMessage.Identifier.GetChannel(),
                sourceMessage.Author.Identifier,
                info.CommandId);
        }

        return true;
    }
}
