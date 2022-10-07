using FloppyBot.Base.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Serilog;

namespace FloppyBot.Base.Testing;

public static class LoggingUtils
{
    private static readonly Lazy<ILoggerFactory> LoggerFactory = new(InitRealLogger);
    private static readonly Lazy<ILoggerFactory> NullLoggerFac = new(NullLoggerFactory.Instance);

    public static bool UseRealLogger { get; set; } = true;

    private static ILoggerFactory InitRealLogger()
    {
        var serilogLogger = new LoggerConfiguration()
            .ConfigureSerilogForTesting()
            .CreateLogger();

        return Microsoft.Extensions.Logging.LoggerFactory.Create(loggingBuilder =>
        {
            loggingBuilder
                .AddSerilog(serilogLogger);
        });
    }

    public static ILogger<T> GetLogger<T>()
    {
        return (UseRealLogger ? LoggerFactory : NullLoggerFac).Value.CreateLogger<T>();
    }
}
