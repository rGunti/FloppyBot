using FloppyBot.Base.Storage.MongoDb;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Driver;

namespace FloppyBot.Tools.V1Migrator.Storage;

public class StorageFactory : IStorageFactory
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<StorageFactory> _logger;
    private readonly IServiceProvider _provider;

    public StorageFactory(ILogger<StorageFactory> logger, IServiceProvider provider, IConfiguration configuration)
    {
        _logger = logger;
        _provider = provider;
        _configuration = configuration;

        BsonSerializer.RegisterIdGenerator(typeof(string), StringObjectIdGenerator.Instance);
        ConventionRegistry.Register("Enum2String", new ConventionPack
        {
            new EnumRepresentationConvention(BsonType.String)
        }, _ => true);
    }

    public MongoUrl GetMongoUrl(StorageSource source)
    {
        return MongoUrl.Create(_configuration.GetConnectionString(source.ToString()));
    }

    public IMongoClient GetMongoClient(StorageSource source)
    {
        return new MongoClient(GetMongoUrl(source));
    }

    public IMongoDatabase GetMongoDatabase(StorageSource source)
    {
        return GetMongoClient(source).GetDatabase(GetMongoUrl(source).DatabaseName);
    }

    public MongoDbRepositoryFactory GetRepositoryFactory(StorageSource source)
    {
        return new MongoDbRepositoryFactory(GetMongoDatabase(source));
    }
}
