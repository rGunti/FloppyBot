using FloppyBot.Aux.MessageCounter.Agent;
using FloppyBot.Aux.MessageCounter.Core;
using FloppyBot.Base.Configuration;
using FloppyBot.Base.Cron;
using FloppyBot.Base.Logging;
using FloppyBot.Base.Storage.MongoDb;
using FloppyBot.Communication.Redis.Config;
using FloppyBot.HealthCheck.Core;
using FloppyBot.HealthCheck.KillSwitch;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

IHostBuilder hostBuilder = Host.CreateDefaultBuilder(args)
    .SetupConfiguration()
    .SetupSerilog();

IHost host = hostBuilder
    .ConfigureServices(services =>
    {
        services
            .AddRedisCommunication()
            .AddCronJobSupport()
            .AddHealthCheck()
            .AddKillSwitchTrigger()
            .AddKillSwitch()
            .AddMessageOccurrenceService()
            .AddMessageCounter()
            .AddMongoDbStorage()
            .AddHostedService<MessageCounterHost>();
    })
    .Build();

await host
    .BootCronJobs()
    .ArmKillSwitch()
    .LogAndRun();
