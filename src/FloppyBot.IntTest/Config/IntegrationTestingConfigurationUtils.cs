using Microsoft.Extensions.Configuration;

namespace FloppyBot.IntTest.Config;

public static class IntegrationTestingConfigurationUtils
{
    public static IConfigurationRoot BuildConfiguration()
    {
        return new ConfigurationBuilder()
            .AddJsonFile("floppytest.json", optional: false)
            .AddJsonFile("floppytest.secret.json", optional: true)
            .AddEnvironmentVariables("FLOPPYTEST_")
            .Build();
    }

    public static IntegrationTestingConfiguration GetIntegrationTestingConfiguration(
        this IConfigurationRoot configuration
    )
    {
        return configuration.Get<IntegrationTestingConfiguration>()
            ?? throw new ArgumentException("Integration testing configuration missing");
    }
}
