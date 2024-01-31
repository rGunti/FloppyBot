using System.Collections.Immutable;
using System.Reflection;
using FloppyBot.Base.Clock;
using FloppyBot.Commands.Core.Entities;
using FloppyBot.Commands.Core.Executor;
using FloppyBot.Commands.Core.Metadata;
using FloppyBot.Commands.Registry;
using FloppyBot.Commands.Registry.Entities;
using Microsoft.Extensions.Logging;

namespace FloppyBot.Commands.Executor.Agent.DistRegistry;

public class DistributedCommandRegistryAdapter
{
    private static readonly string HostProcess = Assembly.GetExecutingAssembly().FullName!;
    private readonly ICommandExecutor _commandExecutor;
    private readonly IDistributedCommandRegistry _distributedCommandRegistry;

    private readonly ILogger<DistributedCommandRegistryAdapter> _logger;
    private readonly IMetadataExtractor _metadataExtractor;
    private readonly ITimeProvider _timeProvider;

    private IImmutableList<CommandAbstract> _storedCommandAbstracts = Array
        .Empty<CommandAbstract>()
        .ToImmutableList();

    public DistributedCommandRegistryAdapter(
        ILogger<DistributedCommandRegistryAdapter> logger,
        IDistributedCommandRegistry distributedCommandRegistry,
        ICommandExecutor commandExecutor,
        ITimeProvider timeProvider,
        IMetadataExtractor metadataExtractor
    )
    {
        _logger = logger;
        _distributedCommandRegistry = distributedCommandRegistry;
        _commandExecutor = commandExecutor;
        _timeProvider = timeProvider;
        _metadataExtractor = metadataExtractor;
    }

    public void ScanAndStoreCommands()
    {
        _storedCommandAbstracts = _commandExecutor
            .KnownCommands.OrderBy(c => c.CommandId)
            .Select(ConvertToAbstract)
            .ToImmutableList();
        _logger.LogInformation(
            "Submitting {CommandCount} known command(s) to distributed command store",
            _storedCommandAbstracts.Count
        );
        foreach (var commandAbstract in _storedCommandAbstracts)
        {
            _distributedCommandRegistry.StoreCommand(commandAbstract.Name, commandAbstract);
        }
    }

    private static CommandParameterAbstractType ToCommandParameterAbstractType(
        CommandParameterType parameterType
    )
    {
        switch (parameterType)
        {
            case CommandParameterType.String:
                return CommandParameterAbstractType.String;
            case CommandParameterType.Number:
                return CommandParameterAbstractType.Number;
            case CommandParameterType.Enum:
                return CommandParameterAbstractType.Enum;
            default:
                throw new ArgumentOutOfRangeException(nameof(parameterType), parameterType, null);
        }
    }

    private CommandAbstract ConvertToAbstract(CommandInfo commandInfo)
    {
        _logger.LogDebug("Extracting metadata for command {CommandInfo}", commandInfo);
        CommandMetadata metadata = _metadataExtractor.ExtractMetadataFromCommand(commandInfo);
        return new CommandAbstract(
            HostProcess,
            _timeProvider.GetCurrentUtcTime(),
            commandInfo.PrimaryCommandName,
            commandInfo.Names.ToArray(),
            metadata.Description,
            metadata.MinPrivilegeLevel,
            metadata.AvailableOnInterfaces,
            metadata.Syntax,
            metadata.HasNoParameters,
            metadata.HiddenCommand,
            metadata
                .Parameters.Select(p => new CommandParameterAbstract(
                    p.Order,
                    p.Name,
                    ToCommandParameterAbstractType(p.Type),
                    p.Required,
                    p.Description,
                    p.PossibleValues
                ))
                .ToArray(),
            metadata.GetRawDataAsDictionary()
        );
    }
}
