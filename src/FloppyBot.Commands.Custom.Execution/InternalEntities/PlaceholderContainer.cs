using FloppyBot.Base.Extensions;
using FloppyBot.Commands.Custom.Storage;
using FloppyBot.Commands.Custom.Storage.Entities;
using FloppyBot.Commands.Parser.Entities;

namespace FloppyBot.Commands.Custom.Execution.InternalEntities;

public interface IPlaceholderContainer
{
    string Caller { get; }
    string[] Params { get; }
    string AllParams { get; }
    string AllParam { get; }
    DateTimeOffset Now { get; }
    int Random { get; }
    int Counter { get; }
    int PeekCounter { get; }
}

public class PlaceholderContainer : IPlaceholderContainer
{
    private readonly CustomCommandDescription _commandDescription;
    private readonly CommandInstruction _commandInstruction;
    private readonly ICounterStorageService _counterStorageService;
    private int? _newCounterValue;

    public PlaceholderContainer(
        CommandInstruction instruction,
        CustomCommandDescription commandDescription,
        DateTimeOffset now,
        int newRandomNumber,
        ICounterStorageService counterStorageService
    )
    {
        _commandInstruction = instruction;
        _commandDescription = commandDescription;

        Params = instruction.Parameters.ToArray();
        AllParams = instruction.Parameters.Join(" ");
        Now = now;
        Random = newRandomNumber;

        _counterStorageService = counterStorageService;
    }

    public string Caller => _commandInstruction.Context!.SourceMessage.Author.DisplayName;
    public string[] Params { get; }
    public string AllParams { get; }
    public string AllParam => AllParams;
    public DateTimeOffset Now { get; }
    public int Random { get; }

    public int Counter
    {
        get
        {
            if (!_newCounterValue.HasValue)
            {
                _newCounterValue = _counterStorageService.Next(_commandDescription.Id);
            }

            return _newCounterValue!.Value;
        }
    }

    public int PeekCounter => _counterStorageService.Peek(_commandDescription.Id);
}
