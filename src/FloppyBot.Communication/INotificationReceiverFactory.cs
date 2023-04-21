namespace FloppyBot.Communication;

public interface INotificationReceiverFactory
{
    INotificationReceiver<T> GetNewReceiver<T>(string connectionString);
}
