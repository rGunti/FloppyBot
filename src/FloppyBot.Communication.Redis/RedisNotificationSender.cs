using System.Text.Json;
using StackExchange.Redis;

namespace FloppyBot.Communication.Redis;

public class RedisNotificationSender : INotificationSender
{
    private readonly ISubscriber _subscriber;
    private readonly string _channel;

    public RedisNotificationSender(ISubscriber subscriber, string channel)
    {
        _subscriber = subscriber;
        _channel = channel;
    }

    public string Channel => _channel;

    public void Send(object obj)
    {
        _subscriber.Publish(RedisChannel.Literal(_channel), JsonSerializer.Serialize(obj));
    }

    public override string ToString()
    {
        return $"{nameof(RedisNotificationSender)}: {_channel}";
    }
}
