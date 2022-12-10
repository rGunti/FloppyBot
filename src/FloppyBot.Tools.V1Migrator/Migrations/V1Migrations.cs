using Microsoft.Extensions.DependencyInjection;

namespace FloppyBot.Tools.V1Migrator.Migrations;

public static class V1Migrations
{
    public static IServiceCollection AddMigrations(this IServiceCollection services)
    {
        return services
            .AddSingleton<IMigration, DropDestinationMigration>()
            .AddSingleton<IMigration, CustomCommandMigration>();
    }
}
