﻿using System.Collections.Immutable;
using System.Linq.Expressions;
using System.Reflection;
using LiteDB;

namespace FloppyBot.Base.Storage.LiteDb;

public class LiteDbRepository<TEntity> : IRepository<TEntity>
    where TEntity : class, IEntity<TEntity>
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
        return _collection.FindById(id);
    }

    public TEntity Insert(TEntity entity)
    {
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (entity.Id == null)
        {
            entity = entity.WithId(Guid.NewGuid().ToString());
        }

        var docId = _collection.Insert(entity);
        return GetById(docId)!;
    }

    public int InsertMany(params TEntity[] entities)
    {
        return _collection.InsertBulk(
            entities
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            .Select(e => e.Id == null ? e.WithId(Guid.NewGuid().ToString()) : e)
        );
    }

    public int InsertMany(IEnumerable<TEntity> entities)
    {
        return InsertMany(entities.ToArray());
    }

    public TEntity Update(TEntity entity)
    {
        _collection.Update(entity);
        return GetById(entity.Id)!;
    }

    public bool Delete(string id)
    {
        return _collection.Delete(id);
    }

    public bool Delete(TEntity entity)
    {
        return Delete(entity.Id);
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

    public TEntity Upsert(TEntity entity)
    {
        _collection.Upsert(entity);
        return GetById(entity.Id)!;
    }

    public TEntity? IncrementField(string id, Expression<Func<TEntity, int>> field, int increment)
    {
        if (
            field.Body
            is not MemberExpression { Member: PropertyInfo memberPropertyInfo } memberExpression
        )
        {
            throw new ArgumentException("Only field expressions are supported", nameof(field));
        }

        var fieldName = memberPropertyInfo.Name;

        var record = _collection.FindById(id);
        if (record is null)
        {
            return null;
        }

        var newFieldValue = field.Compile().Invoke(record) + increment;

        var property = typeof(TEntity).GetProperty(
            fieldName,
            BindingFlags.Instance | BindingFlags.Public
        );
        property!.SetValue(record, newFieldValue);
        return Update(record);
    }
}
