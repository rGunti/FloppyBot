using LiteDB;

namespace FloppyBot.Base.Storage.LiteDb;

public class LiteDbInstanceFactory
{
    public const string IN_MEMORY = ":inmem:";

    public ILiteDatabase ConstructDatabaseInstance(string connectionString)
    {
        return connectionString == IN_MEMORY
            ? ConstructMemoryDbInstance()
            : new LiteDatabase(connectionString);
    }

    public ILiteDatabase ConstructMemoryDbInstance() => new LiteDatabase(ConstructMemoryStream());

    private Stream ConstructMemoryStream() => new MemoryStream();
}
