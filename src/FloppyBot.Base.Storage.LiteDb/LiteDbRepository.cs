using System.Collections.Immutable;
using LiteDB;

namespace FloppyBot.Base.Storage.LiteDb;

public class LiteDbRepository<TEntity> : IRepository<TEntity> where TEntity : class, IEntity
{
    private readonly ILiteCollection<TEntity> _collection;

    public LiteDbRepository(ILiteCollection<TEntity> collection)
    {
        _collection = collection;
    }

    public IEnumerable<TEntity> GetAll()
    {
        return _collection.FindAll();
    }

    public TEntity? GetById(string id)
    {
        return _collection.FindOne(id);
    }

    public TEntity Insert(TEntity entity)
    {
        var docId = _collection.Insert(entity);
        return GetById(docId)!;
    }

    public TEntity Update(TEntity entity)
    {
        _collection.Update(entity);
        return GetById(entity.Id)!;
    }

    public void Delete(string id)
    {
        _collection.Delete(id);
    }

    public void Delete(TEntity entity)
    {
        Delete(entity.Id);
    }

    public int Delete(IEnumerable<string> ids)
    {
        var idSet = ids.ToImmutableHashSet();
        return _collection.DeleteMany(i => idSet.Contains(i.Id));
    }

    public int Delete(IEnumerable<TEntity> entities)
    {
        return Delete(entities.Select(i => i.Id));
    }
}
