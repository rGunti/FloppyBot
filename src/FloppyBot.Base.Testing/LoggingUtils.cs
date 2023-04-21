using FloppyBot.Base.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Serilog;
using ILogger = Serilog.ILogger;

namespace FloppyBot.Base.Testing;

public static class LoggingUtils
{
    private static readonly Lazy<ILoggerFactory> LoggerFactory = new(InitRealLogger);
    private static readonly Lazy<ILoggerFactory> NullLoggerFac = new(NullLoggerFactory.Instance);

    public static readonly Lazy<ILogger> SerilogLogger =
        new(() => new LoggerConfiguration().ConfigureSerilogForTesting().CreateLogger());

    public static bool UseRealLogger { get; set; } = true;

    private static ILoggerFactory InitRealLogger()
    {
        return Microsoft.Extensions.Logging.LoggerFactory.Create(loggingBuilder =>
        {
            loggingBuilder.AddSerilog(SerilogLogger.Value);
        });
    }

    public static ILogger<T> GetLogger<T>()
    {
        return GetLoggerFactory().CreateLogger<T>();
    }

    public static ILoggerFactory GetLoggerFactory()
    {
        return (UseRealLogger ? LoggerFactory : NullLoggerFac).Value;
    }
}
