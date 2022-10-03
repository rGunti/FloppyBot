using FloppyBot.Communication.Redis.Config;
using StackExchange.Redis;

namespace FloppyBot.Communication.Redis;

public class RedisNotificationInterfaceFactory : INotificationReceiverFactory, INotificationSenderFactory
{
    private readonly IRedisConnectionFactory _redisConnectionFactory;

    public RedisNotificationInterfaceFactory(IRedisConnectionFactory redisConnectionFactory)
    {
        _redisConnectionFactory = redisConnectionFactory;
    }

    public INotificationReceiver<T> GetNewReceiver<T>(string connectionString)
    {
        RedisConnectionConfig config = connectionString.ParseToConnectionConfig();
        return new RedisNotificationReceiver<T>(
            _redisConnectionFactory.GetMultiplexer(config).GetSubscriber(),
            config.Channel);
    }

    public INotificationSender GetNewSender(string connectionString)
    {
        RedisConnectionConfig config = connectionString.ParseToConnectionConfig();
        return new RedisNotificationSender(
            _redisConnectionFactory.GetMultiplexer(config).GetSubscriber(),
            config.Channel);
    }
}
