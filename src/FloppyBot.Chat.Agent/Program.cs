using FloppyBot.Base.Configuration;
using FloppyBot.Base.Logging;
using FloppyBot.Chat.Agent;
using FloppyBot.Communication.Redis.Config;

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
            .AddHostedService<ChatAgent>();
    })
    .Build();

await host.RunAsync();
