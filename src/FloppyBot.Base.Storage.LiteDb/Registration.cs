using FloppyBot.Base.Storage.LiteDb.Indexing;
using FloppyBot.Base.Storage.Utils;
using LiteDB;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FloppyBot.Base.Storage.LiteDb;

public static class Registration
{
    public static IServiceCollection AddLiteDbStorage(
        this IServiceCollection services,
        string connectionStringName = "LiteDb"
    )
    {
        return services
            .AddSingleton<LiteDbInstanceFactory>()
            .AddSingleton<ILiteDatabase>(s =>
            {
                var inst = s.GetRequiredService<LiteDbInstanceFactory>()
                    .ConstructDatabaseInstance(
                        s.GetRequiredService<IConfiguration>()
                            .GetConnectionString(connectionStringName)
                        ?? throw new ArgumentException("LiteDb Connection string not found")
                    );
                inst.Pragma("UTC_DATE", true);
                return inst;
            })
            .AddStorageImplementation<LiteDbRepositoryFactory, LiteDbIndexManager>();
    }

    public static IServiceCollection AddInMemoryStorage(this IServiceCollection services)
    {
        return services
            .AddSingleton<LiteDbInstanceFactory>()
            .AddSingleton<ILiteDatabase>(s =>
                s.GetRequiredService<LiteDbInstanceFactory>().ConstructMemoryDbInstance()
            )
            .AddStorageImplementation<LiteDbRepositoryFactory, LiteDbIndexManager>();
    }
}
