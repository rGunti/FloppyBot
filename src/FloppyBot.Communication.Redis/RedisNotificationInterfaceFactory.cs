using StackExchange.Redis;

namespace FloppyBot.Communication.Redis;

public class RedisNotificationInterfaceFactory : INotificationReceiverFactory, INotificationSenderFactory
{
    private readonly IConnectionMultiplexer _connectionMultiplexer;

    public RedisNotificationInterfaceFactory(IConnectionMultiplexer connectionMultiplexer)
    {
        _connectionMultiplexer = connectionMultiplexer;
    }

    public INotificationReceiver<T> GetNewReceiver<T>(string connectionString)
    {
        return new RedisNotificationReceiver<T>(
            _connectionMultiplexer.GetSubscriber(),
            connectionString);
    }

    public INotificationSender GetNewSender(string connectionString)
    {
        return new RedisNotificationSender(
            _connectionMultiplexer.GetSubscriber(),
            connectionString);
    }
}
