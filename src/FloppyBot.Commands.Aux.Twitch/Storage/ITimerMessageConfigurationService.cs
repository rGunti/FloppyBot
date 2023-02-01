using FloppyBot.Base.Extensions;
using FloppyBot.Commands.Aux.Twitch.Storage.Entities;

namespace FloppyBot.Commands.Aux.Twitch.Storage;

public interface ITimerMessageConfigurationService
{
    IEnumerable<TimerMessageConfiguration> GetAllConfigs();
    NullableObject<TimerMessageConfiguration> GetConfigForChannel(string channelId);
    NullableObject<TimerMessageExecution> GetLastExecution(string channelId);
    void UpdateConfigurationForChannel(string channelId, TimerMessageConfiguration configuration);
    void UpdateLastExecutionTime(string channelId, DateTimeOffset executionTime, int messageIndex);
}
