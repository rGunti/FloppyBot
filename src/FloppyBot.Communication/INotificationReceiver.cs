namespace FloppyBot.Communication;

public interface INotificationReceiver<out T>
{
    event NotificationReceivedDelegate<T> NotificationReceived;

    void StartListening();
    void StopListening();
}