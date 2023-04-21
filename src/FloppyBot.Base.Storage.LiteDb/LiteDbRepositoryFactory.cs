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

    public static LiteDbRepositoryFactory CreateMemoryInstance()
    {
        return new LiteDbRepositoryFactory(new LiteDbInstanceFactory().ConstructMemoryDbInstance());
    }

    public IRepository<T> GetRepository<T>()
        where T : class, IEntity<T> =>
        GetRepository<T>(RepositoryFactoryUtils.DetermineCollectionName<T>());

    public IRepository<T> GetRepository<T>(string collectionName)
        where T : class, IEntity<T>
    {
        return new LiteDbRepository<T>(_database.GetCollection<T>(collectionName));
    }
}
