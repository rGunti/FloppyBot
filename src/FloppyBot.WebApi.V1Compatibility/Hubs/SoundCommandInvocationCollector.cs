using AutoMapper;
using FloppyBot.Commands.Custom.Communication;
using FloppyBot.Commands.Custom.Communication.Entities;
using FloppyBot.Communication;
using FloppyBot.WebApi.V1Compatibility.Dtos;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;

namespace FloppyBot.WebApi.V1Compatibility.Hubs;

public class SoundCommandInvocationCollector : IDisposable
{
    private readonly IHubContext<V1SoundCommandHub> _hubContext;
    private readonly IMapper _mapper;
    private readonly INotificationReceiver<SoundCommandInvocation> _receiver;

    public SoundCommandInvocationCollector(
        INotificationReceiverFactory receiverFactory,
        IConfiguration configuration,
        IHubContext<V1SoundCommandHub> hubContext,
        IMapper mapper)
    {
        _receiver = receiverFactory.GetNewReceiver<SoundCommandInvocation>(
            configuration.GetSoundCommandInvocationConfigString());
        _hubContext = hubContext;
        _mapper = mapper;

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
        _hubContext.Clients.Group(notification.InvokedFrom)
            .SendAsync("SoundCommandInvoked", _mapper.Map<InvokeSoundCommandEvent>(notification));
    }
}
