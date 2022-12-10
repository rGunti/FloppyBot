using System.Collections.Immutable;
using FloppyBot.Base.Storage;
using FloppyBot.Base.Storage.MongoDb;
using FloppyBot.Chat.Entities;
using FloppyBot.Commands.Custom.Execution;
using FloppyBot.Commands.Custom.Storage.Entities;
using FloppyBot.Commands.Custom.Storage.Entities.Internal;
using FloppyBot.FileStorage.Entities;
using FloppyBot.Tools.V1Migrator.Config;
using FloppyBot.Tools.V1Migrator.Storage;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;

namespace FloppyBot.Tools.V1Migrator.Migrations;

public class SoundCommandMigration : IMigration
{
    private readonly MigrationConfiguration _configuration;
    private readonly IRepository<CustomCommandDescriptionEo> _destinationRepository;
    private readonly IRepository<FileHeader> _fileHeaderRepository;
    private readonly ILogger<SoundCommandMigration> _logger;
    private readonly IMongoDatabase _sourceDatabase;

    public SoundCommandMigration(
        ILogger<SoundCommandMigration> logger,
        MigrationConfiguration configuration,
        IStorageFactory storageFactory)
    {
        _logger = logger;
        _configuration = configuration;

        _sourceDatabase = storageFactory.GetMongoDatabase(StorageSource.Source);

        MongoDbRepositoryFactory repoFactory = storageFactory.GetRepositoryFactory(StorageSource.Destination);
        _destinationRepository = repoFactory.GetRepository<CustomCommandDescriptionEo>();
        _fileHeaderRepository = repoFactory.GetRepository<FileHeader>();
    }

    public uint Order => 111;

    private IMongoCollection<BsonDocument> SourceSoundCommandTable
        => _sourceDatabase.GetCollection<BsonDocument>("SoundCommand");

    private IMongoCollection<BsonDocument> SourceSoundCommandPayloadTable
        => _sourceDatabase.GetCollection<BsonDocument>("SoundCommandPayload");

    public bool CanExecute() => true;

    public void Execute()
    {
        _logger.LogTrace("Fetching sound commands from source ...");
        ImmutableDictionary<string, BsonDocument> sourceCommands = GetSoundCommandsFromSource()
            .ToImmutableDictionary(d => d["_id"].AsString, d => d);
        _logger.LogInformation(
            "Found {CommandCount} sounds commands to migrate, converting to V2 now ...",
            sourceCommands.Count);

        ImmutableArray<CustomCommandDescriptionEo> convertedCommands = sourceCommands
            .Values
            .Select(ConvertToV2)
            .ToImmutableArray();

        _logger.LogDebug("Converted successfully, verifying sound files ...");
        foreach (CustomCommandDescriptionEo command in convertedCommands)
        {
            VerifyCommand(sourceCommands[command.Id], command);
        }

        _logger.LogDebug("Converted successfully, now writing to destination ...");
        foreach (CustomCommandDescriptionEo command in convertedCommands)
        {
            InsertCommand(command);
        }
    }

    private void VerifyCommand(BsonDocument sourceCommand, CustomCommandDescriptionEo command)
    {
        _logger.LogTrace("Verifying sound command {CommandId} ...", command.Id);
        bool hasLegacyPayload = !sourceCommand["PayloadId"].IsBsonNull;
        if (hasLegacyPayload)
        {
            _logger.LogInformation(
                "Sound command {CommandId} has legacy payload, requires migration of sound file first",
                command.Id);
            MigrateLegacySoundFile(sourceCommand, command);
        }

        _logger.LogDebug("Checking if file header exists for all files in sound command {CommandId}", command.Id);
        ImmutableArray<string> soundFiles = command.Responses
            .Where(response => response.Type == ResponseType.Sound.ToString())
            .Select(response => response.Content.Split(CustomCommandExecutor.SOUND_CMD_SPLIT_CHAR).First())
            .ToImmutableArray();

        foreach (string file in soundFiles)
        {
            _logger.LogDebug("Checking if file {FileId} exists", file);
            if (_fileHeaderRepository.GetById(file) == null)
            {
                _logger.LogError("File {FileId} doesn't exist on destination!", file);
                if (!_configuration.Simulate)
                {
                    throw new Exception($"Cannot continue migration because file {file} doesn't exist on destination");
                }
            }
        }
    }

    private void MigrateLegacySoundFile(BsonDocument sourceCommand, CustomCommandDescriptionEo command)
    {
        throw new NotImplementedException();
    }

    private void InsertCommand(CustomCommandDescriptionEo command)
    {
        _logger.LogTrace("Inserting command {@Command}", command);
        if (!_configuration.Simulate)
        {
            _destinationRepository.Insert(command);
        }
    }

    private IImmutableList<BsonDocument> GetSoundCommandsFromSource()
    {
        if (!_configuration.LimitToChannels.Any())
        {
            return SourceSoundCommandTable
                .Find(Builders<BsonDocument>.Filter.In("ChannelId", _configuration.LimitToChannels))
                .ToList()
                .ToImmutableArray();
        }

        return SourceSoundCommandTable
            .AsQueryable()
            .ToImmutableArray();
    }

    private CustomCommandDescriptionEo ConvertToV2(BsonDocument document)
    {
        return new CustomCommandDescriptionEo
        {
            Id = document["_id"].AsString,
            Name = document["CommandName"].AsString,
            Aliases = Array.Empty<string>(),
            Owners = new[]
            {
                document["ChannelId"].AsString
            },
            Limitations = ConvertPrivilegeLevel(
                document.GetValueIfExists("LimitedToMod")?.AsBoolean ?? false,
                document.GetValueIfExists("Timeout")?.AsInt32 ?? 0),
            ResponseMode = CommandResponseMode.All,
            Responses = new[]
            {
                ConvertCommandResponse(document)
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

    private CommandResponseEo ConvertCommandResponse(BsonDocument document)
    {
        string soundFile;
        bool hasLegacyPayload = !document["PayloadId"].IsBsonNull;
        if (hasLegacyPayload)
        {
            _logger.LogWarning("Command {Id} has legacy payload!", document["_id"].AsString);
            soundFile = document["PayloadId"].AsString;
        }
        else
        {
            soundFile = document["SoundFiles"].AsBsonArray[0].AsString;
        }

        return new CommandResponseEo
        {
            Type = ResponseType.Sound.ToString(),
            Content = GetSoundCommand(soundFile, document["Response"].AsString)
        };
    }

    private static string GetSoundCommand(string file, string response)
    {
        return $"{file}{CustomCommandExecutor.SOUND_CMD_SPLIT_CHAR}{response}";
    }
}

