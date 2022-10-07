using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace FloppyBot.Base.Logging;

public static class SerilogSetup
{
    public static IHostBuilder SetupSerilog(this IHostBuilder hostBuilder)
    {
        return hostBuilder
            .UseSerilog((ctx, lc) => lc.ConfigureSerilog(ctx.Configuration));
    }

    public static LoggerConfiguration ConfigureSerilog(this LoggerConfiguration loggerConfig, IConfiguration hostConfig)
    {
        return loggerConfig
            // - Default Log Configuration
            .Enrich.FromLogContext()
            .Enrich.WithThreadId()
            .WriteTo.Async(s => s
                .Console(
                    outputTemplate:
                    "{Timestamp:HH:mm:ss.fff} | {SourceContext,-75} | {Level:u3} | {Message:lj}{NewLine}{Exception}"))
#if DEBUG
            .MinimumLevel.Verbose()
#else
            .MinimumLevel.Information()
#endif
            // - Configurable via JSON file
            .ReadFrom.Configuration(hostConfig);
    }
}
