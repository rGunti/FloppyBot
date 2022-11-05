using FloppyBot.Base.Storage.Indexing;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;

namespace FloppyBot.Base.Storage.MongoDb.Indexing;

public class MongoDbIndexManager : IIndexManager
{
    private readonly ILogger<MongoDbIndexManager> _logger;
    private readonly IMongoDatabase _mongoDatabase;

    public MongoDbIndexManager(IMongoDatabase mongoDatabase, ILogger<MongoDbIndexManager> logger)
    {
        _mongoDatabase = mongoDatabase;
        _logger = logger;
    }

    private IndexKeysDefinitionBuilder<BsonDocument> IndexBuilder => Builders<BsonDocument>.IndexKeys;

    public bool SupportsIndices => true;

    public bool IndexExists(string collectionName, string indexName, string[] fields)
    {
        if (!CollectionExists(collectionName))
        {
            return false;
        }

        var collection = GetCollection(collectionName);
        return collection.Indexes.List()
            .ToEnumerable()
            .Any(i => i["name"] == indexName);
    }

    public void CreateIndex(string collectionName, string indexName, string[] fields)
    {
        if (!CollectionExists(collectionName))
        {
            _logger.LogInformation("Collection {CollectionName} is missing, creating it ...", collectionName);
            _mongoDatabase.CreateCollection(collectionName);
        }

        var collection = GetCollection(collectionName);

        IndexKeysDefinition<BsonDocument> index;
        if (fields.Length > 1)
        {
            index = IndexBuilder.Ascending(fields[0]);
        }
        else
        {
            index = IndexBuilder.Combine(fields
                .Select(f => Builders<BsonDocument>.IndexKeys.Ascending(f)));
        }

        collection.Indexes.CreateOne(
            new CreateIndexModel<BsonDocument>(
                index,
                new CreateIndexOptions
                {
                    Name = indexName
                }));
    }

    public void DeleteIndex(string collectionName, string indexName)
    {
        var collection = GetCollection(collectionName);
        collection.Indexes.DropOne(indexName);
    }

    private IMongoCollection<BsonDocument> GetCollection(string collectionName) =>
        _mongoDatabase.GetCollection<BsonDocument>(collectionName);

    private bool CollectionExists(string collectionName) =>
        _mongoDatabase.ListCollections().ToEnumerable().Any(i => i["name"] == collectionName);
}
