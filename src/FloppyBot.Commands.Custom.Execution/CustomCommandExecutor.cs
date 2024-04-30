using FloppyBot.Base.Clock;
using FloppyBot.Base.Rng;
using FloppyBot.Base.TextFormatting;
using FloppyBot.Chat.Entities;
using FloppyBot.Chat.Entities.Identifiers;
using FloppyBot.Commands.Core.Cooldown;
using FloppyBot.Commands.Core.Exceptions;
using FloppyBot.Commands.Custom.Communication;
using FloppyBot.Commands.Custom.Communication.Entities;
using FloppyBot.Commands.Custom.Execution.InternalEntities;
using FloppyBot.Commands.Custom.Storage;
using FloppyBot.Commands.Custom.Storage.Entities;
using FloppyBot.Commands.Parser.Entities;
using Microsoft.Extensions.Logging;

namespace FloppyBot.Commands.Custom.Execution;

public interface ICustomCommandExecutor
{
    IEnumerable<string?> Execute(
        CommandInstruction instruction,
        CustomCommandDescription description
    );
}

public class CustomCommandExecutor : ICustomCommandExecutor
{
    private readonly ICooldownService _cooldownService;
    private readonly ICounterStorageService _counterStorageService;
    private readonly ISoundCommandInvocationSender _invocationSender;
    private readonly ILogger<CustomCommandExecutor> _logger;
    private readonly IRandomNumberGenerator _randomNumberGenerator;
    private readonly ITimeProvider _timeProvider;

    public CustomCommandExecutor(
        ILogger<CustomCommandExecutor> logger,
        ITimeProvider timeProvider,
        IRandomNumberGenerator randomNumberGenerator,
        ICooldownService cooldownService,
        ICounterStorageService counterStorageService,
        ISoundCommandInvocationSender invocationSender
    )
    {
        _logger = logger;
        _timeProvider = timeProvider;
        _randomNumberGenerator = randomNumberGenerator;
        _cooldownService = cooldownService;
        _counterStorageService = counterStorageService;
        _invocationSender = invocationSender;
    }

    public IEnumerable<string?> Execute(
        CommandInstruction instruction,
        CustomCommandDescription description
    )
    {
        // Assert Privilege Level
        ChatUser author = instruction.Context!.SourceMessage.Author;
        author.AssertLevel(description.Limitations.MinLevel);

        if (
            description.Limitations.LimitedToUsers.Any()
            && !description.Limitations.LimitedToUsers.Contains(
                author.Identifier.ToString().ToLowerInvariant()
            )
            && !description.Limitations.LimitedToUsers.Contains(
                author.Identifier.Channel.ToLowerInvariant()
            )
        )
        {
            _logger.LogDebug(
                "User {UserId} is not whitelisted for command {CommandId} ({CommandName}), command execution skipped",
                author.Identifier,
                description.Id,
                description.Name
            );
            yield break;
        }

        if (IsOnCooldown(instruction, description))
        {
            _logger.LogDebug(
                "Command {CommandId} ({CommandName}) is currently on cooldown, command execution skipped",
                description.Id,
                description.Name
            );
            yield break;
        }

        // Response
        switch (description.ResponseMode)
        {
            case CommandResponseMode.First:
            case CommandResponseMode.PickOneRandom:
                int index =
                    description.ResponseMode == CommandResponseMode.PickOneRandom
                        ? _randomNumberGenerator.Next(0, description.Responses.Count)
                        : 0;
                yield return Execute(instruction, description, description.Responses[index]);
                break;
            case CommandResponseMode.All:
                foreach (var response in description.Responses)
                {
                    yield return Execute(instruction, description, response);
                }

                break;
            default:
                throw new NotImplementedException(
                    $"Response Mode {description.ResponseMode} not implemented"
                );
        }
    }

    private bool IsOnCooldown(CommandInstruction instruction, CustomCommandDescription description)
    {
        ChatMessage sourceMessage = instruction.Context!.SourceMessage;
        TimeSpan cooldownTime = description
            .Limitations.Cooldown.Where(i => i.Level <= sourceMessage.Author.PrivilegeLevel)
            .OrderByDescending(i => i.Level)
            .Select(i => TimeSpan.FromMilliseconds(i.Milliseconds))
            .FirstOrDefault(TimeSpan.Zero);
        if (cooldownTime == TimeSpan.Zero)
        {
            _logger.LogDebug("No cooldown defined, returning");
            return false;
        }

        DateTimeOffset lastExecution = GetCooldownFor(
            sourceMessage,
            description.Aliases.Concat(new[] { description.Name })
        );
        TimeSpan delta = _timeProvider.GetCurrentUtcTime() - lastExecution;
        if (delta < cooldownTime)
        {
            _logger.LogDebug(
                "Command did not pass cooldown check, delta was {CooldownDelta}, needed at least {Cooldown}",
                delta,
                cooldownTime
            );
            return true;
        }

        return false;
    }

    private DateTimeOffset GetCooldownFor(
        ChatMessage sourceMessage,
        IEnumerable<string> commandNames
    )
    {
        return commandNames
            .Select(commandName =>
                _cooldownService.GetLastExecution(
                    sourceMessage.Identifier.GetChannel(),
                    sourceMessage.Author.Identifier,
                    commandName
                )
            )
            .OrderByDescending(i => i)
            .FirstOrDefault(DateTimeOffset.MinValue);
    }

    private string? Execute(
        CommandInstruction instruction,
        CustomCommandDescription description,
        CommandResponse response
    )
    {
        switch (response.Type)
        {
            case ResponseType.Text:
                return response.Content.Format(
                    new PlaceholderContainer(
                        instruction,
                        description,
                        _timeProvider.GetCurrentUtcTime(),
                        _randomNumberGenerator.Next(0, 100),
                        _counterStorageService
                    )
                );
            case ResponseType.Sound:
                string[] split = response.Content.Split(CommandResponse.SOUND_CMD_SPLIT_CHAR);
                string payloadName = split[0];
                string? reply = split.Length > 1 ? split[1] : null;

                _invocationSender.InvokeSoundCommand(
                    new SoundCommandInvocation(
                        instruction.Context!.SourceMessage.Author.Identifier,
                        instruction.Context!.SourceMessage.Identifier.GetChannel(),
                        description.Name,
                        payloadName,
                        _timeProvider.GetCurrentUtcTime()
                    )
                );
                return reply;
            default:
                throw new NotImplementedException($"Response Type {response.Type} not implemented");
        }
    }
}
