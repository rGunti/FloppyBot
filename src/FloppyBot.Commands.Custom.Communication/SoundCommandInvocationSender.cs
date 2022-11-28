using FloppyBot.Commands.Custom.Communication.Entities;
using FloppyBot.Communication;
using Microsoft.Extensions.Configuration;

namespace FloppyBot.Commands.Custom.Communication;

public class SoundCommandInvocationSender : ISoundCommandInvocationSender
{
    private readonly INotificationSender _notificationSender;

    public SoundCommandInvocationSender(
        INotificationSenderFactory senderFactory,
        IConfiguration configuration)
    {
        _notificationSender = senderFactory.GetNewSender(configuration.GetSoundCommandInvocationConfigString());
    }

    public void InvokeSoundCommand(SoundCommandInvocation invocation)
    {
        _notificationSender.Send(invocation);
    }
}

public class SoundCommandInvocationReceiver : ISoundCommandInvocationReceiver, IDisposable
{
    private readonly INotificationReceiver<SoundCommandInvocation> _receiver;

    public SoundCommandInvocationReceiver(
        INotificationReceiverFactory receiverFactory,
        IConfiguration configuration)
    {
        _receiver = receiverFactory.GetNewReceiver<SoundCommandInvocation>(
            configuration.GetSoundCommandInvocationConfigString());
        _receiver.NotificationReceived += OnNotificationReceived;
        _receiver.StartListening();
    }

    public void Dispose()
    {
        _receiver.StopListening();
    }

    public event SoundCommandInvokedDelegate? SoundCommandInvoked;

    private void OnNotificationReceived(SoundCommandInvocation notification)
    {
        SoundCommandInvoked?.Invoke(this, notification);
    }
}
