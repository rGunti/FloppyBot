namespace FloppyBot.Communication.Mock;

public class MockNotificationReceiver<T> : INotificationReceiver<T>
{
    private readonly string _channel;

    public MockNotificationReceiver(string channel)
    {
        _channel = channel;
    }

    public event NotificationReceivedDelegate<T>? NotificationReceived;

    public void StartListening() { }

    public void StopListening() { }

    public override string ToString()
    {
        return $"{nameof(MockNotificationReceiver<T>)}: {_channel}";
    }
}
