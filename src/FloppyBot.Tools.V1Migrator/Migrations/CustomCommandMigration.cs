using System.Collections.Immutable;
using FloppyBot.Base.Storage;
using FloppyBot.Chat.Entities;
using FloppyBot.Commands.Custom.Storage.Entities;
using FloppyBot.Commands.Custom.Storage.Entities.Internal;
using FloppyBot.Tools.V1Migrator.Storage;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;

namespace FloppyBot.Tools.V1Migrator.Migrations;

public class CustomCommandMigration : IMigration
{
    private readonly IRepository<CustomCommandDescriptionEo> _destinationRepository;
    private readonly ILogger<CustomCommandMigration> _logger;
    private readonly IMongoDatabase _mongoDatabase;

    public CustomCommandMigration(
        ILogger<CustomCommandMigration> logger,
        IStorageFactory storageFactory)
    {
        _logger = logger;
        _mongoDatabase = storageFactory.GetMongoDatabase(StorageSource.Source);
        _destinationRepository = storageFactory.GetRepositoryFactory(StorageSource.Destination)
            .GetRepository<CustomCommandDescriptionEo>();
    }

    public uint Order => 1;

    private IMongoCollection<BsonDocument> SourceCollection
        => _mongoDatabase.GetCollection<BsonDocument>("CustomCommand");

    public void Execute()
    {
        _logger.LogTrace("Fetching commands from source...");
        IImmutableList<BsonDocument> sourceCommands = GetCommandsFromSource();

        _logger.LogDebug("Found {CommandCount} commands on source, converting to V2 now ...", sourceCommands.Count);
        ImmutableArray<CustomCommandDescriptionEo> convertedCommands = sourceCommands
            .Select(ConvertToV2)
            .ToImmutableArray();

        _logger.LogDebug("Converted successfully, now inserting into destination ...");
        foreach (CustomCommandDescriptionEo command in convertedCommands)
        {
            _logger.LogTrace("Inserting command {Command}", command);
            _destinationRepository.Insert(command);
        }
    }

    private IImmutableList<BsonDocument> GetCommandsFromSource() => SourceCollection.AsQueryable().ToImmutableArray();

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
