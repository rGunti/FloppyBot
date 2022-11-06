using FloppyBot.Base.EquatableCollections;
using FloppyBot.Chat.Entities;
using FloppyBot.Commands.Registry;
using FloppyBot.Commands.Registry.Entities;
using FloppyBot.WebApi.V1Compatibility.Dtos;
using Microsoft.Extensions.Logging;

namespace FloppyBot.WebApi.V1Compatibility.Services;

public class V1CommandConverter
{
    private readonly IDistributedCommandRegistry _distributedCommandRegistry;
    private readonly ILogger<V1CommandConverter> _logger;

    public V1CommandConverter(IDistributedCommandRegistry distributedCommandRegistry,
        ILogger<V1CommandConverter> logger)
    {
        _distributedCommandRegistry = distributedCommandRegistry;
        _logger = logger;
    }

    public IEnumerable<CommandInfo> GetAllKnownCommands()
    {
        return _distributedCommandRegistry.GetAllCommands()
            .Select(ConvertToCommandInfo);
    }

    private CommandInfo ConvertToCommandInfo(CommandAbstract commandAbstract)
    {
        _logger.LogDebug("Converting command {Command} to V1 Command Info", commandAbstract.Name);
        return new CommandInfo(
            commandAbstract.Name,
            commandAbstract.Description ?? string.Empty,
            commandAbstract.Aliases.ToImmutableListWithValueSemantics(),
            commandAbstract.AvailableOnInterfaces.ToImmutableListWithValueSemantics(),
            commandAbstract.MinPrivilegeLevel ?? PrivilegeLevel.Unknown,
            false,
            // TODO: Implement once available
            new CooldownInfo("None", null, null, null),
            (commandAbstract.Syntax ?? Array.Empty<string>())
            .Select(s => new CommandSyntax(
                s,
                string.Empty,
                Array.Empty<string>().ToImmutableListWithValueSemantics()))
            .ToImmutableListWithValueSemantics(),
            false);
    }
}
