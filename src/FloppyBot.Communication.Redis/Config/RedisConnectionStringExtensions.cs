using StackExchange.Redis;

namespace FloppyBot.Communication.Redis.Config;

public static class RedisConnectionStringExtensions
{
    public static RedisConnectionConfig ParseToConnectionConfig(this string connectionString)
    {
        string[] split = connectionString.Split('|');
        if (split.Length != 2)
        {
            throw new ArgumentException(
                "Provided connection string does not contain all parts required to be parsed",
                nameof(connectionString));
        }
        return new RedisConnectionConfig(
            ConfigurationOptions.Parse(split[0]),
            split[1]);
    }
}
