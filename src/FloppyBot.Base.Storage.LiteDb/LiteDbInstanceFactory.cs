using LiteDB;

namespace FloppyBot.Base.Storage.LiteDb;

public class LiteDbInstanceFactory
{
    public const string IN_MEMORY = ":inmem:";
    private Stream ConstructMemoryStream() => new MemoryStream();

    public ILiteDatabase ConstructDatabaseInstance(string connectionString)
    {
        return connectionString == IN_MEMORY ? ConstructMemoryDbInstance() : new LiteDatabase(connectionString);
    }

    public ILiteDatabase ConstructMemoryDbInstance() => new LiteDatabase(ConstructMemoryStream());
}
