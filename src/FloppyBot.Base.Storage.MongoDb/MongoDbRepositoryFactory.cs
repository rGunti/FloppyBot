using FloppyBot.Base.Storage.Utils;
using MongoDB.Driver;

namespace FloppyBot.Base.Storage.MongoDb;

public class MongoDbRepositoryFactory : IRepositoryFactory
{
    private readonly IMongoDatabase _database;

    public MongoDbRepositoryFactory(IMongoDatabase database)
    {
        _database = database;
    }

    public IRepository<T> GetRepository<T>() where T : class, IEntity<T>
        => GetRepository<T>(RepositoryFactoryUtils.DetermineCollectionName<T>());

    public IRepository<T> GetRepository<T>(string collectionName) where T : class, IEntity<T>
    {
        return new MongoDbRepository<T>(_database.GetCollection<T>(collectionName));
    }
}
