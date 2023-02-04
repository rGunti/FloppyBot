using System.Reflection;
using FloppyBot.Base.Clock;
using FloppyBot.Base.Extensions;
using FloppyBot.Chat.Entities;
using FloppyBot.Chat.Entities.Identifiers;
using FloppyBot.Commands.Core.Attributes.Metadata;
using FloppyBot.Commands.Core.Config;
using FloppyBot.Commands.Core.Cooldown;
using FloppyBot.Commands.Core.Entities;
using FloppyBot.Commands.Parser.Entities;
using Microsoft.Extensions.Logging;

namespace FloppyBot.Commands.Core.Support.Hybrid;

[TaskOrder(ORDER)]
public class CooldownTask : IHybridTask
{
    private const int ORDER = 20;
    private readonly ICommandConfigurationService _commandConfigurationService;
    private readonly ICooldownService _cooldownService;

    private readonly ILogger<CooldownTask> _logger;
    private readonly ITimeProvider _timeProvider;

    public CooldownTask(
        ILogger<CooldownTask> logger,
        ICooldownService cooldownService,
        ITimeProvider timeProvider,
        ICommandConfigurationService commandConfigurationService)
    {
        _logger = logger;
        _cooldownService = cooldownService;
        _timeProvider = timeProvider;
        _commandConfigurationService = commandConfigurationService;
    }

    public bool ExecutePre(CommandInfo info, CommandInstruction instruction)
    {
        ChatMessage sourceMessage = instruction.Context!.SourceMessage;
        TimeSpan cooldownTime = DetermineCooldown(info, sourceMessage);
        if (cooldownTime == TimeSpan.Zero)
        {
            _logger.LogDebug("No cooldown defined, skipped");
            return true;
        }

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

    private TimeSpan DetermineCooldown(CommandInfo info, ChatMessage sourceMessage)
    {
        PrivilegeLevel userPrivilegeLevel = sourceMessage.Author.PrivilegeLevel;
        return ExtractCooldownFromConfiguration(
                sourceMessage.Identifier.GetChannel(),
                info.CommandId,
                userPrivilegeLevel)
            .Concat(ExtractCooldownFromCommandImplementation(info, userPrivilegeLevel))
            .FirstOrDefault(TimeSpan.Zero);
    }

    private IEnumerable<TimeSpan> ExtractCooldownFromConfiguration(
        string channelId,
        string command,
        PrivilegeLevel userPrivilegeLevel)
    {
        return _commandConfigurationService
            .GetCommandConfiguration(channelId, command)
            .Where(config => config.CustomCooldown)
            .SelectMany(config => config.CustomCooldownConfiguration)
            .Where(cooldownConfig => userPrivilegeLevel <= cooldownConfig.PrivilegeLevel)
            .MaxBy(cooldownConfig => cooldownConfig.PrivilegeLevel)
            .Wrap()
            .Select(cooldownConfig => TimeSpan.FromMilliseconds(cooldownConfig.CooldownMs));
    }

    private IEnumerable<TimeSpan> ExtractCooldownFromCommandImplementation(
        CommandInfo info,
        PrivilegeLevel userPrivilegeLevel)
    {
        return info.HandlerMethod
            .GetCustomAttributes<CommandCooldownAttribute>()
            .Where(a => userPrivilegeLevel <= a.MaxLevel)
            .MaxBy(a => a.MaxLevel)
            .Wrap()
            .Select(i => TimeSpan.FromMilliseconds(i.CooldownMs));
    }
}
