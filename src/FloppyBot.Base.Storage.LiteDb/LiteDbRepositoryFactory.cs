using FloppyBot.Base.Storage.Utils;
using LiteDB;

namespace FloppyBot.Base.Storage.LiteDb;

public class LiteDbRepositoryFactory : IRepositoryFactory
{
    private readonly ILiteDatabase _database;

    public LiteDbRepositoryFactory(ILiteDatabase database)
    {
        _database = database;
    }

    public IRepository<T> GetRepository<T>() where T : class, IEntity
        => GetRepository<T>(RepositoryFactoryUtils.DetermineCollectionName<T>());

    public IRepository<T> GetRepository<T>(string collectionName) where T : class, IEntity
    {
        return new LiteDbRepository<T>(_database.GetCollection<T>(collectionName));
    }
}
