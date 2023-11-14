using FloppyBot.Base.Logging;
using Microsoft.Extensions.Logging;
using Serilog;
using Xunit.Abstractions;

namespace FloppyBot.IntTest.Utils;

public static class LoggingUtils
{
    public static ILoggerFactory GetXunitLogger(this ITestOutputHelper output)
    {
        return LoggerFactory.Create(loggingBuilder =>
        {
            loggingBuilder.AddSerilog(
                new LoggerConfiguration()
                    .ConfigureCommonSerilogSettings(null)
                    .WriteTo
                    .Xunit(output, outputTemplate: SerilogSetup.OUTPUT_TEMPLATE)
                    .MinimumLevel
                    .Verbose()
                    .CreateLogger()
            );
        });
    }
}
