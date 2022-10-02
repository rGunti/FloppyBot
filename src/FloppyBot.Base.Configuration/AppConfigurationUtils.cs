using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

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
}
