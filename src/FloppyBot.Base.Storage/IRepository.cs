namespace FloppyBot.Base.Storage;

public interface IRepository<TEntity> where TEntity : class, IEntity<TEntity>
{
    IEnumerable<TEntity> GetAll();
    TEntity? GetById(string id);
    TEntity Insert(TEntity entity);
    int InsertMany(IEnumerable<TEntity> entities);
    int InsertMany(params TEntity[] entities);
    TEntity Update(TEntity entity);
    bool Delete(string id);
    bool Delete(TEntity entity);
    int Delete(IEnumerable<string> ids);
    int Delete(IEnumerable<TEntity> entities);
}
