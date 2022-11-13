using System.Reflection;
using FloppyBot.Base.Clock;
using FloppyBot.Chat.Entities;
using FloppyBot.Chat.Entities.Identifiers;
using FloppyBot.Commands.Core.Attributes.Metadata;
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
        CommandCooldownAttribute? cooldown = ExtractCooldown(info, sourceMessage.Author.PrivilegeLevel);
        if (cooldown == null)
        {
            _logger.LogDebug("No cooldown defined, skipped");
            return true;
        }

        TimeSpan cooldownTime = TimeSpan.FromMilliseconds(cooldown.CooldownMs);

        DateTimeOffset lastExecution = _cooldownService.GetLastExecution(
            sourceMessage.Identifier.GetChannel(),
            sourceMessage.Author.Identifier,
            info.CommandId);
        TimeSpan delta = _timeProvider.GetCurrentUtcTime() - lastExecution;
        if (delta < cooldownTime)
        {
            _logger.LogDebug(
                "Last execution did not pass cooldown check, delta was {CooldownDelta}, needed at least {Cooldown}",
                delta,
                cooldownTime);
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

    private CommandCooldownAttribute? ExtractCooldown(CommandInfo info, PrivilegeLevel userPrivilegeLevel)
    {
        return info.HandlerMethod
            .GetCustomAttributes<CommandCooldownAttribute>()
            .Where(a => userPrivilegeLevel <= a.MaxLevel)
            .MaxBy(a => a.MaxLevel);
    }
}
