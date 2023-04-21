namespace FloppyBot.Communication.Mock;

public class MockNotificationSender : INotificationSender
{
    private readonly string _channel;

    public MockNotificationSender(string channel)
    {
        _channel = channel;
    }

    public List<object> SentMessages { get; } = new();

    public event NotificationReceivedDelegate<object>? NotificationSent;

    public void Send(object obj)
    {
        SentMessages.Add(obj);
        NotificationSent?.Invoke(obj);
    }

    public override string ToString()
    {
        return $"{nameof(MockNotificationSender)}: {_channel}";
    }
}
