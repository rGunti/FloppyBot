namespace FloppyBot.Communication.Mock;

public class MockNotificationInterfaceFactory : INotificationReceiverFactory, INotificationSenderFactory
{
    public INotificationReceiver<T> GetNewReceiver<T>(string connectionString)
    {
        return new MockNotificationReceiver<T>(connectionString);
    }

    public INotificationSender GetNewSender(string connectionString)
    {
        return new MockNotificationSender(connectionString);
    }
}
