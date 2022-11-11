using FloppyBot.Base.Clock;
using FloppyBot.Base.TextFormatting;
using FloppyBot.Commands.Core.Exceptions;
using FloppyBot.Commands.Custom.Storage.Entities;
using FloppyBot.Commands.Parser.Entities;
using Microsoft.Extensions.Logging;

namespace FloppyBot.Commands.Custom.Execution;

public interface ICustomCommandExecutor
{
    IEnumerable<string?> Execute(CommandInstruction instruction, CustomCommandDescription description);
}

public class CustomCommandExecutor
{
    private static readonly Random Rng = new();

    private readonly ILogger<CustomCommandExecutor> _logger;
    private readonly ITimeProvider _timeProvider;

    public CustomCommandExecutor(
        ILogger<CustomCommandExecutor> logger,
        ITimeProvider timeProvider)
    {
        _logger = logger;
        _timeProvider = timeProvider;
    }

    public IEnumerable<string?> Execute(CommandInstruction instruction, CustomCommandDescription description)
    {
        // Assert Privilege Level
        var author = instruction.Context!.SourceMessage.Author;
        author.AssertLevel(description.Limitations.MinLevel);

        // TODO: Assert Cooldown

        // Response
        switch (description.ResponseMode)
        {
            case CommandResponseMode.First:
            case CommandResponseMode.PickOneRandom:
                var index = description.ResponseMode == CommandResponseMode.PickOneRandom
                    ? Rng.Next(description.Responses.Count)
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
                    Random = Rng.Next(100),
                });
            default:
                throw new NotImplementedException($"Response Type {response.Type} not implemented");
        }
    }
}
