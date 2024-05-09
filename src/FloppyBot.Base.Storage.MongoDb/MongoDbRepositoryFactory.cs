using FloppyBot.Base.Storage.Utils;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;

namespace FloppyBot.Base.Storage.MongoDb;

public class MongoDbRepositoryFactory : IRepositoryFactory
{
    private readonly ILogger<MongoDbRepositoryFactory> _logger;
    private readonly IMongoDatabase _database;

    public MongoDbRepositoryFactory(
        IMongoDatabase database,
        ILogger<MongoDbRepositoryFactory> logger
    )
    {
        _database = database;
        _logger = logger;
    }

    public IRepository<T> GetRepository<T>()
        where T : class, IEntity<T> =>
        GetRepository<T>(RepositoryFactoryUtils.DetermineCollectionName<T>());

    public IRepository<T> GetRepository<T>(string collectionName)
        where T : class, IEntity<T>
    {
        var collectionExists = _database
            .ListCollections(
                new ListCollectionsOptions
                {
                    Filter = Builders<BsonDocument>.Filter.Eq(f => f["name"], collectionName),
                }
            )
            .Any();
        if (!collectionExists)
        {
            _logger.LogDebug("Creating collection {CollectionName}", collectionName);
            _database.CreateCollection(collectionName);
        }

        return new MongoDbRepository<T>(_database.GetCollection<T>(collectionName));
    }
}
