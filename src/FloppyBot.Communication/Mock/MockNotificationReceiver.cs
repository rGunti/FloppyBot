namespace FloppyBot.Communication.Mock;

public class MockNotificationReceiver<T> : INotificationReceiver<T>
{
    public MockNotificationReceiver(string channel)
    {
        Channel = channel;
    }

    public string Channel { get; }

    public event NotificationReceivedDelegate<T>? NotificationReceived;

    public void StartListening() { }

    public void StopListening() { }

    public override string ToString()
    {
        return $"{nameof(MockNotificationReceiver<T>)}: {Channel}";
    }
}
