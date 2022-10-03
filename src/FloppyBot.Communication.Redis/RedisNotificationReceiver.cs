using System.Text.Json;
using StackExchange.Redis;

namespace FloppyBot.Communication.Redis;

public class RedisNotificationReceiver<T> : INotificationReceiver<T>
{
    private readonly ISubscriber _subscriber;
    private readonly string _channel;

    private bool _isStarted;

    public RedisNotificationReceiver(ISubscriber subscriber, string channel)
    {
        _subscriber = subscriber;
        _channel = channel;
    }

    public event NotificationReceivedDelegate<T>? NotificationReceived;

    public string Channel => _channel;
    public bool IsStarted => _isStarted;
    
    public void StartListening()
    {
        if (_isStarted)
        {
            return;
        }

        _subscriber.Subscribe(_channel, HandleNewMessage);
        _isStarted = true;
    }

    private void HandleNewMessage(RedisChannel _, RedisValue value)
    {
        var data = JsonSerializer.Deserialize<T>(value.ToString());
        if (data != null)
        {
            NotificationReceived?.Invoke(data);
        }
    }

    public void StopListening()
    {
        if (!_isStarted)
        {
            return;
        }

        _subscriber.Unsubscribe(_channel, HandleNewMessage);
        _isStarted = false;
    }

    public override string ToString()
    {
        return $"{nameof(RedisNotificationReceiver<T>)}<{typeof(T)}>: {_channel}";
    }
}
