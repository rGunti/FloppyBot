namespace FloppyBot.Communication;

public interface INotificationSenderFactory
{
    INotificationSender GetNewSender(string connectionString);
}