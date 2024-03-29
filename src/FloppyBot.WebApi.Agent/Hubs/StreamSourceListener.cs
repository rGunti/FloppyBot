using FloppyBot.Commands.Custom.Communication;
using FloppyBot.Commands.Custom.Communication.Entities;
using FloppyBot.Communication;
using Microsoft.AspNetCore.SignalR;

namespace FloppyBot.WebApi.Agent.Hubs;

public class StreamSourceListener : IDisposable
{
    private readonly IHubContext<StreamSourceHub> _hubContext;
    private readonly INotificationReceiver<SoundCommandInvocation> _receiver;

    public StreamSourceListener(
        IHubContext<StreamSourceHub> hubContext,
        INotificationReceiverFactory receiverFactory,
        IConfiguration configuration
    )
    {
        _hubContext = hubContext;
        _receiver = receiverFactory.GetNewReceiver<SoundCommandInvocation>(
            configuration.GetSoundCommandInvocationConfigString()
        );

        _receiver.NotificationReceived += OnNotificationReceived;
        _receiver.StartListening();
    }

    public void Dispose()
    {
        _receiver.StopListening();
        _receiver.NotificationReceived -= OnNotificationReceived;
    }

    private void OnNotificationReceived(SoundCommandInvocation notification)
    {
        _hubContext
            .Clients.Group(notification.InvokedFrom)
            .SendCoreAsync("SoundCommandReceived", [notification]);
    }
}
