using System.Collections.Immutable;
using System.Reflection;
using FloppyBot.Base.Clock;
using FloppyBot.Commands.Core.Attributes.Metadata;
using FloppyBot.Commands.Core.Entities;
using FloppyBot.Commands.Core.Executor;
using FloppyBot.Commands.Core.Metadata;
using FloppyBot.Commands.Registry;
using FloppyBot.Commands.Registry.Entities;
using Microsoft.Extensions.Logging;

namespace FloppyBot.Commands.Executor.Agent.DistRegistry;

public class DistributedCommandRegistryAdapter : IDisposable
{
    private static readonly string HostProcess = Assembly.GetExecutingAssembly().FullName!;
    private readonly ICommandExecutor _commandExecutor;
    private readonly IDistributedCommandRegistry _distributedCommandRegistry;

    private readonly ILogger<DistributedCommandRegistryAdapter> _logger;
    private readonly IMetadataExtractor _metadataExtractor;
    private readonly ITimeProvider _timeProvider;

    private IImmutableList<CommandAbstract> _storedCommandAbstracts = Array.Empty<CommandAbstract>().ToImmutableList();

    public DistributedCommandRegistryAdapter(
        ILogger<DistributedCommandRegistryAdapter> logger,
        IDistributedCommandRegistry distributedCommandRegistry,
        ICommandExecutor commandExecutor,
        ITimeProvider timeProvider,
        IMetadataExtractor metadataExtractor)
    {
        _logger = logger;
        _distributedCommandRegistry = distributedCommandRegistry;
        _commandExecutor = commandExecutor;
        _timeProvider = timeProvider;
        _metadataExtractor = metadataExtractor;
    }

    public void Dispose()
    {
        _logger.LogInformation(
            "Removing {CommandCount} command(s) from distributed command store",
            _storedCommandAbstracts.Count);
        foreach (var commandAbstract in _storedCommandAbstracts)
        {
            _distributedCommandRegistry.RemoveCommand(commandAbstract.Name);
        }
    }

    public void Start()
    {
        _storedCommandAbstracts = _commandExecutor.KnownCommands
            .SelectMany(ConvertToAbstract)
            .ToImmutableList();
        _logger.LogInformation(
            "Submitting {CommandCount} known command(s) to distributed command store",
            _storedCommandAbstracts.Count);
        foreach (var commandAbstract in _storedCommandAbstracts)
        {
            _distributedCommandRegistry.StoreCommand(commandAbstract.Name, commandAbstract);
        }
    }

    private IEnumerable<CommandAbstract> ConvertToAbstract(CommandInfo commandInfo)
    {
        _logger.LogDebug("Extracting metadata for command {CommandInfo}", commandInfo);
        var metadata = _metadataExtractor.ExtractMetadataFromCommand(commandInfo);
        var commandAbstract = new CommandAbstract(
            HostProcess,
            _timeProvider.GetCurrentUtcTime(),
            string.Empty, // <- filled later
            commandInfo.Names.ToArray(),
            metadata.Description,
            metadata.MinPrivilegeLevel,
            metadata.AvailableOnInterfaces,
            metadata.GetRawDataAsDictionary());

        // If a primary name is known, just emit that
        if (metadata.HasValue(CommandMetadataTypes.PRIMARY_NAME))
        {
            yield return commandAbstract with
            {
                Name = metadata[CommandMetadataTypes.PRIMARY_NAME]
            };
        }

        // otherwise emit every name individually
        foreach (var commandInfoName in commandInfo.Names)
        {
            yield return commandAbstract with
            {
                Name = commandInfoName
            };
        }
    }
}
