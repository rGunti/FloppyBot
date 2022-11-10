using FloppyBot.Communication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FloppyBot.HealthCheck.KillSwitch;

public class KillSwitchTriggerSender : IKillSwitchTrigger
{
    private readonly ILogger<KillSwitchTriggerSender> _logger;
    private readonly INotificationSender _sender;

    public KillSwitchTriggerSender(
        ILogger<KillSwitchTriggerSender> logger,
        INotificationSenderFactory senderFactory,
        IConfiguration configuration)
    {
        _logger = logger;
        _sender = senderFactory.GetNewSender(
            configuration.GetKillSwitchConnectionString());
    }

    public void RequestRestart(string instanceId)
    {
        _sender.Send(new KillSwitchMessage(instanceId));
    }
}
