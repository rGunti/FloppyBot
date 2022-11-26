using FloppyBot.Base.Clock;
using FloppyBot.Base.Rng;
using FloppyBot.Base.TextFormatting;
using FloppyBot.Chat.Entities;
using FloppyBot.Chat.Entities.Identifiers;
using FloppyBot.Commands.Core.Cooldown;
using FloppyBot.Commands.Core.Exceptions;
using FloppyBot.Commands.Custom.Storage.Entities;
using FloppyBot.Commands.Parser.Entities;
using Microsoft.Extensions.Logging;

namespace FloppyBot.Commands.Custom.Execution;

public interface ICustomCommandExecutor
{
    IEnumerable<string?> Execute(CommandInstruction instruction, CustomCommandDescription description);
}

public class CustomCommandExecutor : ICustomCommandExecutor
{
    private readonly ICooldownService _cooldownService;
    private readonly ILogger<CustomCommandExecutor> _logger;
    private readonly IRandomNumberGenerator _randomNumberGenerator;
    private readonly ITimeProvider _timeProvider;

    public CustomCommandExecutor(
        ILogger<CustomCommandExecutor> logger,
        ITimeProvider timeProvider,
        IRandomNumberGenerator randomNumberGenerator,
        ICooldownService cooldownService)
    {
        _logger = logger;
        _timeProvider = timeProvider;
        _randomNumberGenerator = randomNumberGenerator;
        _cooldownService = cooldownService;
    }

    public IEnumerable<string?> Execute(CommandInstruction instruction, CustomCommandDescription description)
    {
        // Assert Privilege Level
        ChatUser author = instruction.Context!.SourceMessage.Author;
        author.AssertLevel(description.Limitations.MinLevel);

        if (IsOnCooldown(instruction, description))
        {
            yield break;
        }

        // Response
        switch (description.ResponseMode)
        {
            case CommandResponseMode.First:
            case CommandResponseMode.PickOneRandom:
                int index = description.ResponseMode == CommandResponseMode.PickOneRandom
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
                throw new NotImplementedException($"Response Mode {description.ResponseMode} not implemented");
        }
    }

    private bool IsOnCooldown(CommandInstruction instruction, CustomCommandDescription description)
    {
        ChatMessage sourceMessage = instruction.Context!.SourceMessage;
        TimeSpan cooldownTime = description.Limitations.Cooldown
            .Where(i => i.Level <= sourceMessage.Author.PrivilegeLevel)
            .OrderByDescending(i => i.Level)
            .Select(i => TimeSpan.FromMilliseconds(i.Milliseconds))
            .FirstOrDefault(TimeSpan.Zero);
        if (cooldownTime == TimeSpan.Zero)
        {
            _logger.LogDebug("No cooldown defined, returning");
            return false;
        }

        DateTimeOffset lastExecution =
            GetCooldownFor(sourceMessage, description.Aliases.Concat(new[] { description.Name }));
        TimeSpan delta = _timeProvider.GetCurrentUtcTime() - lastExecution;
        if (delta < cooldownTime)
        {
            _logger.LogDebug(
                "Command did not pass cooldown check, delta was {CooldownDelta}, needed at least {Cooldown}",
                delta,
                cooldownTime);
            return true;
        }

        return false;
    }

    private DateTimeOffset GetCooldownFor(
        ChatMessage sourceMessage,
        IEnumerable<string> commandNames)
    {
        return commandNames
            .Select(commandName => _cooldownService.GetLastExecution(
                sourceMessage.Identifier.GetChannel(),
                sourceMessage.Author.Identifier, commandName))
            .OrderByDescending(i => i)
            .FirstOrDefault(DateTimeOffset.MinValue);
    }

    private string? Execute(
        CommandInstruction instruction,
        CustomCommandDescription description,
        CommandResponse response)
    {
        switch (response.Type)
        {
            case ResponseType.Text:
                return response.Content.Format(new
                {
                    Caller = instruction.Context!.SourceMessage.Author.DisplayName,
                    Params = instruction.Parameters,
                    AllParams = string.Join(" ", instruction.Parameters),
                    Now = _timeProvider.GetCurrentUtcTime(),
                    Random = _randomNumberGenerator.Next(0, 100),
                    Counter = -1,
                });
            default:
                throw new NotImplementedException($"Response Type {response.Type} not implemented");
        }
    }
}
