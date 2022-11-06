using System.Collections.Immutable;
using System.Text.Json;
using FloppyBot.Base.Configuration;
using FloppyBot.Base.EquatableCollections;
using FloppyBot.Commands.Registry.Entities;
using FloppyBot.Communication.Redis;
using FloppyBot.Communication.Redis.Config;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;

namespace FloppyBot.Commands.Registry;

public class DistributedCommandRegistry : IDistributedCommandRegistry
{
    private const string GLOBAL_KEY = "FloppyBot.Commands";

    private readonly IDatabase _database;
    private readonly string _globalKey;

    public DistributedCommandRegistry(
        IConfiguration configuration,
        IRedisConnectionFactory connectionFactory)
    {
        var connectionString = configuration.GetParsedConnectionString("DistributedCommandRegistry");
        var config = connectionString.ParseToConnectionConfig();
        _globalKey = config.Channel ?? GLOBAL_KEY;

        _database = connectionFactory.GetMultiplexer(connectionString)
            .GetDatabase();
    }

    public IImmutableList<string> GetCommands()
    {
        return _database.HashKeys(_globalKey)
            .Where(v => !v.IsNull)
            .Select(v => v.ToString())
            .ToImmutableListWithValueSemantics();
    }

    public IImmutableList<CommandAbstract> GetAllCommands()
    {
        return _database.HashValues(_globalKey)
            .Where(v => !v.IsNull)
            .Select(v => JsonSerializer.Deserialize<CommandAbstract>(v.ToString())!)
            .ToImmutableList();
    }

    public CommandAbstract? GetCommand(string commandName)
    {
        return GetCommandFromDb(commandName);
    }

    public void StoreCommand(string commandName, CommandAbstract commandAbstract)
    {
        AddCommandToDb(commandName, commandAbstract);
    }

    public void RemoveCommand(string commandName)
    {
        _database.HashDelete(_globalKey, commandName);
    }

    private IEnumerable<string> GetCommandNames()
    {
        return _database.HashKeys(_globalKey)
            .Where(v => !v.IsNull)
            .Select(v => v.ToString());
    }

    private CommandAbstract? GetCommandFromDb(string commandName)
    {
        var command = _database.HashGet(_globalKey, commandName);
        if (command.IsNull)
        {
            return null;
        }

        return JsonSerializer.Deserialize<CommandAbstract?>(command.ToString());
    }

    private void AddCommandToDb(string commandName, CommandAbstract command)
    {
        _database.HashSet(_globalKey, commandName, JsonSerializer.Serialize(command));
    }
}
