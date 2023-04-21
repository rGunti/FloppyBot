using FloppyBot.Base.Configuration;
using FloppyBot.Base.Cron;
using FloppyBot.Base.Logging;
using FloppyBot.Chat;
using FloppyBot.Chat.Console.Agent;
using FloppyBot.Communication.Redis.Config;
using FloppyBot.HealthCheck.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

IHostBuilder builder = Host.CreateDefaultBuilder(args).SetupConfiguration().SetupSerilog();

IHost host = builder
    .ConfigureServices(services =>
    {
        services
            .AddRedisCommunication()
            .AddSingleton(
                s =>
                    s.GetRequiredService<IConfiguration>()
                        .GetSection("UserConfig")
                        .Get<ConsoleAgentUserConfiguration>()
            )
            .AddCronJobSupport()
            .AddHealthCheck()
            .AddSingleton<IChatInterface, ConsoleChatInterface>()
            .AddSingleton<ConsoleChatInterface>()
            .AddHostedService<ConsoleChatAgent>();
    })
    .Build();

await host.BootCronJobs().RunAsync();
