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
