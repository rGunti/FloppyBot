using FloppyBot.Base.Storage.Utils;
using LiteDB;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FloppyBot.Base.Storage.LiteDb;

public static class Registration
{
    public static IServiceCollection AddLiteDbStorage(
        this IServiceCollection services,
        string connectionStringName = "LiteDb")
    {
        return services
            .AddSingleton<LiteDbInstanceFactory>()
            .AddSingleton<ILiteDatabase>(s =>
            {
                var inst = s.GetRequiredService<LiteDbInstanceFactory>()
                    .ConstructDatabaseInstance(s.GetRequiredService<IConfiguration>()
                        .GetConnectionString(connectionStringName));
                inst.Pragma("UTC_DATE", true);
                return inst;
            })
            .AddStorageImplementation<LiteDbRepositoryFactory>();
    }
}

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
