namespace FloppyBot.Base.Storage;

public interface IEntity<out TEntity> where TEntity : IEntity<TEntity>
{
    public string Id { get; }
    public TEntity WithId(string newId);
}
