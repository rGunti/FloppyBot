using System.Linq.Expressions;
using MongoDB.Driver;

namespace FloppyBot.Base.Storage.MongoDb;

public class MongoDbRepository<TEntity> : IRepository<TEntity>
    where TEntity : class, IEntity<TEntity>
{
    private readonly IMongoCollection<TEntity> _collection;

    public MongoDbRepository(IMongoCollection<TEntity> collection)
    {
        _collection = collection;
    }

    private FilterDefinitionBuilder<TEntity> Filter => Builders<TEntity>.Filter;

    public IEnumerable<TEntity> GetAll()
    {
        return _collection.AsQueryable();
    }

    public TEntity? GetById(string id)
    {
        return _collection.Find(GetIdFilter(id)).FirstOrDefault();
    }

    public TEntity Insert(TEntity entity)
    {
        _collection.InsertOne(entity);
        return entity;
    }

    public int InsertMany(IEnumerable<TEntity> entities)
    {
        return InsertMany(entities.ToArray());
    }

    public int InsertMany(params TEntity[] entities)
    {
        _collection.InsertMany(entities);
        return entities.Length;
    }

    public TEntity Update(TEntity entity)
    {
        _collection.ReplaceOne(GetIdFilter(entity.Id), entity);
        return entity;
    }

    public bool Delete(string id)
    {
        return _collection.DeleteOne(GetIdFilter(id)).DeletedCount > 0L;
    }

    public bool Delete(TEntity entity)
    {
        return Delete(entity.Id);
    }

    public int Delete(IEnumerable<string> ids)
    {
        return (int)_collection.DeleteMany(Filter.In(i => i.Id, ids.ToHashSet())).DeletedCount;
    }

    public int Delete(IEnumerable<TEntity> entities)
    {
        return Delete(entities.Select(i => i.Id));
    }

    public TEntity Upsert(TEntity entity)
    {
        var result = _collection.ReplaceOne(
            GetIdFilter(entity.Id),
            entity,
            new ReplaceOptions { IsUpsert = true }
        );
        if (!result.IsAcknowledged)
        {
            throw new Exception($"Could not upsert entity {typeof(TEntity).Name} {entity}");
        }

        return GetById(entity.Id)!;
    }

    public TEntity? IncrementField(string id, Expression<Func<TEntity, int>> field, int increment)
    {
        var update = Builders<TEntity>.Update.Inc(field, increment);
        var result = _collection.UpdateOne(GetIdFilter(id), update);
        if (result.MatchedCount == 0)
        {
            return null;
        }

        return GetById(id);
    }

    private FilterDefinition<TEntity> GetIdFilter(string id) => Filter.Eq(i => i.Id, id);
}
