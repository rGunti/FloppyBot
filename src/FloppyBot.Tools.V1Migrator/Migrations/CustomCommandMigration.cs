using System.Collections.Immutable;
using FloppyBot.Base.Storage;
using FloppyBot.Chat.Entities;
using FloppyBot.Commands.Custom.Storage.Entities;
using FloppyBot.Commands.Custom.Storage.Entities.Internal;
using FloppyBot.Tools.V1Migrator.Config;
using FloppyBot.Tools.V1Migrator.Storage;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;

namespace FloppyBot.Tools.V1Migrator.Migrations;

public class CustomCommandMigration : IMigration
{
    private readonly MigrationConfiguration _configuration;
    private readonly IRepository<CustomCommandDescriptionEo> _destinationRepository;
    private readonly ILogger<CustomCommandMigration> _logger;
    private readonly IMongoDatabase _mongoDatabase;

    public CustomCommandMigration(
        ILogger<CustomCommandMigration> logger,
        IStorageFactory storageFactory,
        MigrationConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
        _mongoDatabase = storageFactory.GetMongoDatabase(StorageSource.Source);
        _destinationRepository = storageFactory.GetRepositoryFactory(StorageSource.Destination)
            .GetRepository<CustomCommandDescriptionEo>();
    }

    public uint Order => 1;

    private IMongoCollection<BsonDocument> SourceCollection
        => _mongoDatabase.GetCollection<BsonDocument>("CustomCommand");

    public bool CanExecute() => true;

    public void Execute()
    {
        _logger.LogTrace("Fetching commands from source...");
        IImmutableList<BsonDocument> sourceCommands = GetCommandsFromSource();

        _logger.LogInformation("Found {CommandCount} commands on source, converting to V2 now ...",
            sourceCommands.Count);
        ImmutableArray<CustomCommandDescriptionEo> convertedCommands = sourceCommands
            .Select(ConvertToV2)
            .ToImmutableArray();

        _logger.LogDebug("Converted successfully, now writing to destination ...");
        foreach (CustomCommandDescriptionEo command in convertedCommands)
        {
            InsertCommand(command);
        }
    }

    private void InsertCommand(CustomCommandDescriptionEo command)
    {
        _logger.LogTrace("Inserting command {Command}", command);
        if (!_configuration.Simulate)
        {
            _destinationRepository.Insert(command);
        }
    }

    private IImmutableList<BsonDocument> GetCommandsFromSource()
    {
        if (_configuration.LimitToChannels.Any())
        {
            return SourceCollection
                .Find(Builders<BsonDocument>.Filter.In("Channel", _configuration.LimitToChannels))
                .ToList()
                .ToImmutableArray();
        }

        return SourceCollection.AsQueryable().ToImmutableArray();
    }

    private static CustomCommandDescriptionEo ConvertToV2(BsonDocument document)
    {
        return new CustomCommandDescriptionEo
        {
            Id = document["_id"].AsString,
            Name = document["Command"].AsString,
            Aliases = Array.Empty<string>(),
            Limitations = ConvertPrivilegeLevel(
                document.GetValueIfExists("LimitedToMod")?.AsBoolean ?? false,
                document.GetValueIfExists("Timeout")?.AsInt32 ?? 0),
            Owners = new[]
            {
                document["Channel"].AsString
            },
            ResponseMode = CommandResponseMode.All,
            Responses = new[]
            {
                new CommandResponseEo
                {
                    Type = ResponseType.Text.ToString(),
                    Content = document["Response"].AsString
                }
            }
        };
    }

    private static CommandLimitationEo ConvertPrivilegeLevel(bool limitedToMod, int cooldown)
    {
        return new CommandLimitationEo
        {
            MinLevel = limitedToMod ? PrivilegeLevel.Moderator : PrivilegeLevel.Viewer,
            Cooldown = new[]
            {
                new CooldownDescriptionEo
                {
                    Level = PrivilegeLevel.Viewer,
                    Milliseconds = cooldown
                }
            }
        };
    }
}

