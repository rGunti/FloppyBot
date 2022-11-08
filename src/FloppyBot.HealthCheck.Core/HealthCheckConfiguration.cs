using FloppyBot.Base.Configuration;
using Microsoft.Extensions.Configuration;

namespace FloppyBot.HealthCheck.Core;

public static class HealthCheckConfiguration
{
    public const string HEALTH_CHECK_CONNECTION_STRING_NAME = "HealthCheck";
    public const string INSTANCE_NAME_KEY = "InstanceName";

    public static string GetHealthCheckConnectionString(this IConfiguration configuration)
    {
        return configuration.GetParsedConnectionString(HEALTH_CHECK_CONNECTION_STRING_NAME);
    }

    public static string GetInstanceName(this IConfiguration configuration)
    {
        return configuration[INSTANCE_NAME_KEY] ?? "Unnamed Instance";
    }
}
