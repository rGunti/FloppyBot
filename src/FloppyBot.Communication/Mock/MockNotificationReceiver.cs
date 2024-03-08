namespace FloppyBot.Communication.Mock;

public class MockNotificationReceiver<T> : INotificationReceiver<T>
{
    public MockNotificationReceiver(string channel)
    {
        Channel = channel;
    }

    public string Channel { get; }

#pragma warning disable CS0067
    public event NotificationReceivedDelegate<T>? NotificationReceived;
#pragma warning restore CS0067

    public void StartListening() { }

    public void StopListening() { }

    public override string ToString()
    {
        return $"{nameof(MockNotificationReceiver<T>)}: {Channel}";
    }
}
