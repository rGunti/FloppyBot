using FloppyBot.Communication.Redis.Config;
using Microsoft.Extensions.Logging;

namespace FloppyBot.Communication.Redis;

public class RedisNotificationInterfaceFactory
    : INotificationReceiverFactory,
        INotificationSenderFactory
{
    private readonly ILogger<RedisNotificationInterfaceFactory> _logger;
    private readonly IRedisConnectionFactory _redisConnectionFactory;

    public RedisNotificationInterfaceFactory(
        ILogger<RedisNotificationInterfaceFactory> logger,
        IRedisConnectionFactory redisConnectionFactory
    )
    {
        _logger = logger;
        _redisConnectionFactory = redisConnectionFactory;
    }

    public INotificationReceiver<T> GetNewReceiver<T>(string connectionString)
    {
        _logger.LogDebug("Creating new receiver for {ConnectionString}", connectionString);
        RedisConnectionConfig config = connectionString.ParseToConnectionConfig();
        return new RedisNotificationReceiver<T>(
            _redisConnectionFactory.GetMultiplexer(config).GetSubscriber(),
            config.Channel
        );
    }

    public INotificationSender GetNewSender(string connectionString)
    {
        _logger.LogDebug("Creating new sender for {ConnectionString}", connectionString);
        RedisConnectionConfig config = connectionString.ParseToConnectionConfig();
        return new RedisNotificationSender(
            _redisConnectionFactory.GetMultiplexer(config).GetSubscriber(),
            config.Channel
        );
    }
}
