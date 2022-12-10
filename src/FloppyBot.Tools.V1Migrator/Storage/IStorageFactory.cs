using FloppyBot.Base.Storage.MongoDb;
using MongoDB.Driver;

namespace FloppyBot.Tools.V1Migrator.Storage;

public interface IStorageFactory
{
    MongoUrl GetMongoUrl(StorageSource source);
    IMongoClient GetMongoClient(StorageSource source);
    IMongoDatabase GetMongoDatabase(StorageSource source);
    MongoDbRepositoryFactory GetRepositoryFactory(StorageSource source);
}

