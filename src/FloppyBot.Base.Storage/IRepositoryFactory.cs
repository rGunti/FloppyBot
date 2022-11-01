namespace FloppyBot.Base.Storage;

public interface IRepositoryFactory
{
    IRepository<T> GetRepository<T>() where T : class, IEntity<T>;
    IRepository<T> GetRepository<T>(string collectionName) where T : class, IEntity<T>;
}
