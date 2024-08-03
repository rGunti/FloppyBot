using System.Linq.Expressions;

namespace FloppyBot.Base.Storage;

public interface IRepository<TEntity>
    where TEntity : class, IEntity<TEntity>
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
    TEntity Upsert(TEntity entity);
    TEntity? IncrementField(string id, Expression<Func<TEntity, int>> field, int increment);
}
