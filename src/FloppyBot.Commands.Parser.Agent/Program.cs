using FloppyBot.Base.Configuration;
using FloppyBot.Base.Logging;
using FloppyBot.Commands.Parser;
using FloppyBot.Commands.Parser.Agent;
using FloppyBot.Communication.Redis.Config;

IHostBuilder builder = Host.CreateDefaultBuilder(args)
    .SetupConfiguration()
    .SetupSerilog();

IHost host = builder
    .ConfigureServices(services =>
    {
        services
            .AddRedisCommunication()
            .AddSingleton<ICommandParser, CommandParser>(s =>
            {
                var config = s.GetRequiredService<IConfiguration>();
                string[] prefixes = config.GetSection("CommandPrefixes").Get<string[]>() ?? new[] { "?" };
                return new CommandParser(prefixes);
            })
            .AddHostedService<CommandParsingAgent>();
    })
    .Build();

await host.LogAndRun();
