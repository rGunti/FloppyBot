using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Configuration;

namespace FloppyBot.Base.Logging;

public static class SerilogSetup
{
    private const string OUTPUT_TEMPLATE =
        "{Timestamp:HH:mm:ss.fff} | {SourceContext,-75} | {Level:u3} | {Message:lj}{NewLine}{Exception}";

    public static IHostBuilder SetupSerilog(this IHostBuilder hostBuilder)
    {
        return hostBuilder.UseSerilog((ctx, lc) => lc.ConfigureSerilog(ctx.Configuration));
    }

    public static LoggerConfiguration ConfigureSerilog(
        this LoggerConfiguration loggerConfig,
        IConfiguration hostConfig
    )
    {
        return loggerConfig
            // - Default Log Configuration
            .ConfigureCommonSerilogSettings()
            .WriteTo.Async(s => s.CommonConsoleOutput())
#if DEBUG
            .MinimumLevel.Verbose()
#else
            .MinimumLevel.Information()
#endif
            // - Configurable via JSON file
            .ReadFrom.Configuration(hostConfig);
    }

    public static LoggerConfiguration ConfigureSerilogForTesting(
        this LoggerConfiguration loggerConfig
    )
    {
        return loggerConfig
            .ConfigureCommonSerilogSettings()
            .WriteTo.CommonConsoleOutput()
            .MinimumLevel.Verbose();
    }

    internal static LoggerConfiguration ConfigureCommonSerilogSettings(
        this LoggerConfiguration loggerConfig
    )
    {
        return loggerConfig.Enrich.FromLogContext().Enrich.WithThreadId();
    }

    internal static LoggerConfiguration CommonConsoleOutput(this LoggerSinkConfiguration sinkConfig)
    {
        return sinkConfig.Console(outputTemplate: OUTPUT_TEMPLATE);
    }
}
