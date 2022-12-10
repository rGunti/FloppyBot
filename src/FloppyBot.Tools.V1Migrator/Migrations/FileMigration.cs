using System.Collections.Immutable;
using FloppyBot.Base.Storage;
using FloppyBot.Base.Storage.MongoDb;
using FloppyBot.FileStorage.Entities;
using FloppyBot.Tools.V1Migrator.Config;
using FloppyBot.Tools.V1Migrator.Storage;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;

namespace FloppyBot.Tools.V1Migrator.Migrations;

public class FileMigration : IMigration
{
    private readonly MigrationConfiguration _configuration;
    private readonly IRepository<FileContent> _fileContentRepository;
    private readonly IRepository<FileHeader> _fileHeaderRepository;
    private readonly ILogger<FileMigration> _logger;
    private readonly IMongoDatabase _sourceDatabase;

    public FileMigration(
        ILogger<FileMigration> logger,
        MigrationConfiguration configuration,
        IStorageFactory storageFactory)
    {
        _logger = logger;
        _configuration = configuration;
        _sourceDatabase = storageFactory.GetMongoDatabase(StorageSource.Source);
        MongoDbRepositoryFactory repoFactory = storageFactory.GetRepositoryFactory(StorageSource.Destination);
        _fileHeaderRepository = repoFactory.GetRepository<FileHeader>();
        _fileContentRepository = repoFactory.GetRepository<FileContent>();
    }

    public uint Order => 110;

    private IMongoCollection<BsonDocument> SourceFileHeaderTable
        => _sourceDatabase.GetCollection<BsonDocument>("FileHeader");

    private IMongoCollection<BsonDocument> SourceFileContentTable
        => _sourceDatabase.GetCollection<BsonDocument>("FilePayload");

    public bool CanExecute() => true;

    public void Execute()
    {
        _logger.LogTrace("Fetching file headers from source ...");
        IImmutableList<BsonDocument> sourceHeaders = GetSourceFileHeaders();
        _logger.LogInformation(
            "Found {FileHeaderCount} file headers to migrate, converting to V2 now ...",
            sourceHeaders.Count);

        ImmutableArray<FileHeader> convertedHeaders = sourceHeaders
            .Select(ConvertHeaderToV2)
            .ToImmutableArray();

        _logger.LogDebug("Converted successfully, preparing to load file headers ...");
        foreach (FileHeader fileHeader in convertedHeaders)
        {
            ConvertFile(fileHeader);
        }
    }

    private void ConvertFile(FileHeader fileHeader)
    {
        _logger.LogTrace("Converting file {@FileHeader}", fileHeader);
        BsonDocument? sourceFileContent = FetchFileContentFromSource(fileHeader.Id);

        if (sourceFileContent == null)
        {
            _logger.LogWarning(
                "Did not find file content for {FileHeaderId}!",
                fileHeader.Id);
            return;
        }

        _logger.LogDebug("Converting content for {FileId}", fileHeader.Id);
        FileContent content = ConvertContentToV2(sourceFileContent);

        InsertFile(fileHeader, content);
    }

    private void InsertFile(FileHeader header, FileContent content)
    {
        if (!_configuration.Simulate)
        {
            _logger.LogTrace("Inserting file content for file {FileId}", header.Id);
            _fileContentRepository.Insert(content);
            _logger.LogTrace("Inserting file header for file {FileId}", header.Id);
            _fileHeaderRepository.Insert(header);
        }
    }

    private BsonDocument? FetchFileContentFromSource(string fileId)
    {
        _logger.LogTrace("Loading file {FileId}", fileId);
        return SourceFileContentTable
            .Find(Builders<BsonDocument>.Filter.Eq("_id", fileId))
            .FirstOrDefault();
    }

    private IImmutableList<BsonDocument> GetSourceFileHeaders()
    {
        if (!_configuration.LimitToChannels.Any())
        {
            return SourceFileHeaderTable
                .Find(Builders<BsonDocument>.Filter.In("ChannelId", _configuration.LimitToChannels))
                .ToList()
                .ToImmutableArray();
        }

        return SourceFileHeaderTable
            .AsQueryable()
            .ToImmutableArray();
    }

    private FileHeader ConvertHeaderToV2(BsonDocument document)
    {
        return new FileHeader(
            document["_id"].AsString,
            document["ChannelId"].AsString,
            document["FileName"].AsString,
            document["FileSize"].AsInt64,
            document["MimeType"].AsString);
    }

    private FileContent ConvertContentToV2(BsonDocument document)
    {
        return new FileContent(
            document["_id"].AsString,
            document["Content"].AsByteArray);
    }
}


