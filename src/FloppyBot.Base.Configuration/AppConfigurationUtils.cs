using System.Collections.Immutable;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using SmartFormat;

namespace FloppyBot.Base.Configuration;

public static class AppConfigurationUtils
{
    public static string CurrentEnvironment => Environment.GetEnvironmentVariable("FLOPPY_ENV") ?? "Production";

    public static IConfigurationBuilder SetupCommonConfig(
        this IConfigurationBuilder builder,
        string? environment = null)
    {
        return builder
            .AddJsonFile("floppybot.json", optional: false)
            .AddJsonFile($"floppybot.{environment ?? CurrentEnvironment}.json", optional: true)
            .AddEnvironmentVariables("FLOPPYBOT_");
    }

    public static IConfiguration BuildCommonConfig(string? environment = null)
    {
        return new ConfigurationBuilder()
            .SetupCommonConfig(environment)
            .Build();
    }

    public static IHostBuilder SetupConfiguration(this IHostBuilder hostBuilder)
    {
        return hostBuilder
            .ConfigureAppConfiguration((_, c) => c.SetupCommonConfig());
    }

    public static IReadOnlyDictionary<string, string> GetConnectionStrings(this IConfiguration configuration)
    {
        return configuration
            .GetSection("ConnectionStrings")
            .GetChildren()
            .ToImmutableDictionary(i => i.Key, i => i.Value);
    }

    public static string GetParsedConnectionString(
        this IConfiguration configuration,
        string name)
    {
        return configuration.GetConnectionString(name).FormatSmart(configuration.GetConnectionStrings());
    }
}
