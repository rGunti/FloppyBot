using FloppyBot.Base.Configuration;
using FloppyBot.Base.Logging;
using FloppyBot.Tools.V1Migrator;
using FloppyBot.Tools.V1Migrator.Migrations;
using FloppyBot.Tools.V1Migrator.Storage;
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
            .AddHostedService<MigrationHost>();
    })
    .Build();

await host.LogAndRun();
