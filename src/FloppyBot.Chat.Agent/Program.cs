using FloppyBot.Base.Configuration;
using FloppyBot.Base.Cron;
using FloppyBot.Base.Logging;
using FloppyBot.Chat.Agent;
using FloppyBot.Communication.Redis.Config;
using FloppyBot.HealthCheck.Core;
using FloppyBot.HealthCheck.KillSwitch;

IConfiguration config = AppConfigurationUtils.BuildCommonConfig();

IHostBuilder builder = Host.CreateDefaultBuilder(args)
    // - Config
    .SetupConfiguration()
    // - Logging
    .SetupSerilog();

IHost host = builder
    .ConfigureServices(services =>
    {
        services
            .RegisterChatInterface(config.GetValue<string>("InterfaceType"))
            .AddRedisCommunication()
            .AddCronJobSupport()
            .AddHealthCheck()
            .AddKillSwitch()
            .AddHostedService<ChatAgent>();
    })
    .Build();

await host
    .BootCronJobs()
    .ArmKillSwitch()
    .LogAndRun();
