using FloppyBot.Base.BinLoader;
using FloppyBot.Base.Configuration;
using FloppyBot.Base.Cron;
using FloppyBot.Base.Logging;
using FloppyBot.Base.Storage.MongoDb;
using FloppyBot.Commands.Core.Scan;
using FloppyBot.Commands.Executor.Agent;
using FloppyBot.Commands.Executor.Agent.DistRegistry;
using FloppyBot.Commands.Registry;
using FloppyBot.Communication.Redis.Config;
using FloppyBot.HealthCheck.Core;
using FloppyBot.HealthCheck.KillSwitch;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

// Force-load all dependencies
AssemblyPreloader.LoadAssembliesFromDirectory();

IHostBuilder builder = Host.CreateDefaultBuilder(args)
    .SetupConfiguration()
    .SetupSerilog();

IHost host = builder
    .ConfigureServices(services =>
    {
        services
            .AddRedisCommunication()
            .AddMongoDbStorage()
            .ScanAndAddCommandDependencies()
            .AddDistributedCommandRegistry()
            .AddCronJobSupport()
            .AddHealthCheck()
            .AddKillSwitch()
            .AddSingleton<DistributedCommandRegistryAdapter>()
            .AddCronJob<DistributedCommandRegistryCronJob>()
            .AddHostedService<ExecutorAgent>();
    })
    .Build();

await host
    .BootCronJobs()
    .ArmKillSwitch()
    .LogAndRun();
