using FloppyBot.Base.Configuration;
using Microsoft.Extensions.Configuration;

namespace FloppyBot.HealthCheck.KillSwitch;

public static class KillSwitchConfiguration
{
    public const string TOPIC_NAME = "KillSwitch";

    public static string GetKillSwitchConnectionString(this IConfiguration configuration)
    {
        return configuration.GetParsedConnectionString(TOPIC_NAME);
    }
}
