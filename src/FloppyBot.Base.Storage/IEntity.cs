namespace FloppyBot.Base.Storage;

public interface IEntity
{
    public string Id { get; }
}

public interface IEntity<out TEntity> : IEntity
    where TEntity : IEntity<TEntity>
{
    public TEntity WithId(string newId);
}
