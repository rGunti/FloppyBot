using FloppyBot.Base.Configuration;
using FloppyBot.Base.Logging;
using FloppyBot.Commands;
using FloppyBot.Commands.Executor.Agent;
using FloppyBot.Commands.Executor.Agent.Cmds;
using FloppyBot.Communication.Redis.Config;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

IHostBuilder builder = Host.CreateDefaultBuilder(args)
    .SetupConfiguration()
    .SetupSerilog();

IHost host = builder
    .ConfigureServices(services =>
    {
        services
            .AddRedisCommunication()
            .AddScoped<IBotCommand, PingCommand>()
            .AddHostedService<ExecutorAgent>();
    })
    .Build();

await host.RunAsync();
