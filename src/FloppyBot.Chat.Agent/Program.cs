using FloppyBot.Base.Configuration;
using FloppyBot.Chat.Agent;
using Serilog;

IConfiguration config = AppConfigurationUtils.BuildCommonConfig();

IHostBuilder builder = Host.CreateDefaultBuilder(args)
    // - Config
    .SetupConfiguration()
    // - Logging
    .UseSerilog((ctx, lc) => lc
        // - Default Log Configuration
        .Enrich.FromLogContext()
        .Enrich.WithThreadId()
        .WriteTo.Async(s => s
            .Console(
                outputTemplate:
                "{Timestamp:HH:mm:ss.fff} | {SourceContext,-75} | {Level:u3} | {Message:lj}{NewLine}{Exception}"))
        // - Configurable via JSON file
        .ReadFrom.Configuration(ctx.Configuration));

IHost host = builder
    .ConfigureServices(services =>
    {
        services
            .RegisterChatInterface(config.GetValue<string>("InterfaceType"))
            .AddHostedService<ChatAgent>();
    })
    .Build();

await host.RunAsync();
