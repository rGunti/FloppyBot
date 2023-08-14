using FloppyBot.Aux.TwitchAlerts.Agent;
using FloppyBot.Aux.TwitchAlerts.Core;
using FloppyBot.Base.Configuration;
using FloppyBot.Base.Logging;
using FloppyBot.Base.Storage.MongoDb;
using FloppyBot.Communication.Redis.Config;
using FloppyBot.HealthCheck.Core;
using FloppyBot.HealthCheck.KillSwitch;

IHostBuilder hostBuilder = Host.CreateDefaultBuilder(args).SetupConfiguration().SetupSerilog();

IHost host = hostBuilder
    .ConfigureServices(services =>
    {
        services
            .AddRedisCommunication()
            .AddHealthCheck()
            .AddKillSwitchTrigger()
            .AddKillSwitch()
            .AddMongoDbStorage()
            .AddTwitchAlertService()
            .AddTwitchAlertCore()
            .AddHostedService<TwitchAlertHost>();
    })
    .Build();

await host.ArmKillSwitch().LogAndRun();
