using FloppyBot.Base.Logging.Enrichers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Configuration;

namespace FloppyBot.Base.Logging;

public static class SerilogSetup
{
    public const string OUTPUT_TEMPLATE =
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
            .ConfigureCommonSerilogSettings(hostConfig)
            .WriteTo
            .Async(s => s.CommonConsoleOutput())
#if DEBUG
            .MinimumLevel
            .Verbose()
#else
            .MinimumLevel
            .Information()
#endif
            // - Configurable via JSON file
            .ReadFrom
            .Configuration(hostConfig);
    }

    public static LoggerConfiguration ConfigureSerilogForTesting(
        this LoggerConfiguration loggerConfig,
        IConfiguration? hostConfig = null
    )
    {
        return loggerConfig
            .ConfigureCommonSerilogSettings(hostConfig)
            .WriteTo
            .CommonConsoleOutput()
            .MinimumLevel
            .Verbose();
    }

    public static LoggerConfiguration ConfigureCommonSerilogSettings(
        this LoggerConfiguration loggerConfig,
        IConfiguration? hostConfig
    )
    {
        var state = loggerConfig
            .Enrich
            .FromLogContext()
            .Enrich
            .WithThreadId()
            .Enrich
            .WithAssemblyName()
            .Enrich
            .WithAssemblyVersion()
            .Enrich
            .WithAssemblyInformationalVersion();
        if (hostConfig is not null)
        {
            state = state.Enrich.With(new InstanceNameEnricher(hostConfig));
        }

        return state;
    }

    internal static LoggerConfiguration CommonConsoleOutput(this LoggerSinkConfiguration sinkConfig)
    {
        return sinkConfig.Console(outputTemplate: OUTPUT_TEMPLATE);
    }
}
