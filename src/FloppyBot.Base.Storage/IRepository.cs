namespace FloppyBot.Base.Storage;

public interface IRepository<TEntity> where TEntity : class, IEntity<TEntity>
{
    IEnumerable<TEntity> GetAll();
    TEntity? GetById(string id);
    TEntity Insert(TEntity entity);
    TEntity Update(TEntity entity);
    bool Delete(string id);
    bool Delete(TEntity entity);
    int Delete(IEnumerable<string> ids);
    int Delete(IEnumerable<TEntity> entities);
}
