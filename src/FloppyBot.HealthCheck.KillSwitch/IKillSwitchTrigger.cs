namespace FloppyBot.HealthCheck.KillSwitch;

public interface IKillSwitchTrigger
{
    void RequestRestart(string instanceId);
}
