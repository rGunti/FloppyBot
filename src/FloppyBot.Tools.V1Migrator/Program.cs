using FloppyBot.Base.Configuration;
using FloppyBot.Base.Logging;
using FloppyBot.Tools.V1Migrator;
using FloppyBot.Tools.V1Migrator.Config;
using FloppyBot.Tools.V1Migrator.Migrations;
using FloppyBot.Tools.V1Migrator.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

IHostBuilder builder = Host.CreateDefaultBuilder(args)
    .SetupConfiguration()
    .SetupSerilog();

IHost host = builder
    .ConfigureServices(services =>
    {
        services
            .AddSingleton<IStorageFactory, StorageFactory>()
            .AddMigrations()
            .AddSingleton<MigrationConfiguration>(s =>
            {
                var instance = new MigrationConfiguration();
                s.GetRequiredService<IConfiguration>()
                    .GetSection("Migration")
                    .Bind(instance);
                return instance;
            })
            .AddHostedService<MigrationHost>();
    })
    .Build();

await host.LogAndRun();
