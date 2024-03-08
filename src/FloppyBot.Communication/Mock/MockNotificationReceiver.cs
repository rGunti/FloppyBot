namespace FloppyBot.Communication.Mock;

public class MockNotificationReceiver<T> : INotificationReceiver<T>
{
    public MockNotificationReceiver(string channel)
    {
        Channel = channel;
    }

    public string Channel { get; }

#pragma warning disable 0169
    public event NotificationReceivedDelegate<T>? NotificationReceived;
#pragma warning restore 0169

    public void StartListening() { }

    public void StopListening() { }

    public override string ToString()
    {
        return $"{nameof(MockNotificationReceiver<T>)}: {Channel}";
    }
}
