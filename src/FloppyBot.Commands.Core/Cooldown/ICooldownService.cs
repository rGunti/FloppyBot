namespace FloppyBot.Commands.Core.Cooldown;

public interface ICooldownService
{
    DateTimeOffset GetLastExecution(string channelId, string userId, string commandId);
    void StoreExecution(string channelId, string userId, string commandId);
}
