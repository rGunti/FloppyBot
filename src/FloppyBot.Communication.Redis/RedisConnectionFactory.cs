using FloppyBot.Communication.Redis.Config;
using StackExchange.Redis;

namespace FloppyBot.Communication.Redis;

public class RedisConnectionFactory : IRedisConnectionFactory
{
    public IConnectionMultiplexer GetMultiplexer(RedisConnectionConfig connectionConfig)
    {
        return ConnectionMultiplexer.Connect(connectionConfig.Options);
    }
}

public interface IRedisConnectionFactory
{
    IConnectionMultiplexer GetMultiplexer(RedisConnectionConfig connectionConfig);
}

public static class RedisConnectionFactoryExtensions
{
    public static IConnectionMultiplexer GetMultiplexer(
        this IRedisConnectionFactory connectionFactory,
        string connectionString)
    {
        return connectionFactory.GetMultiplexer(
            connectionString.ParseToConnectionConfig());
    }
}
