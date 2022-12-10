using FloppyBot.Tools.V1Migrator.Storage;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace FloppyBot.Tools.V1Migrator.Migrations;

public class DropDestinationMigration : IMigration
{
    private readonly IMongoClient _client;
    private readonly string _database;
    private readonly ILogger<DropDestinationMigration> _logger;

    public DropDestinationMigration(
        ILogger<DropDestinationMigration> logger,
        IStorageFactory storageFactory)
    {
        _logger = logger;
        _client = storageFactory.GetMongoClient(StorageSource.Destination);
        _database = storageFactory.GetMongoUrl(StorageSource.Destination).DatabaseName;
    }

    public uint Order => uint.MinValue;

    public void Execute()
    {
        _logger.LogInformation("Dropping database {DatabaseName}", _database);
        _client.DropDatabase(_database);
    }
}

