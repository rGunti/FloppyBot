using FloppyBot.Commands.Aux.Twitch.Storage.Entities;

namespace FloppyBot.Commands.Aux.Twitch.Storage;

public interface IShoutoutMessageSettingService
{
    ShoutoutMessageSetting? GetSettings(string channelId);
    void SetShoutoutMessage(string channelId, string message);
    void ClearSettings(string channelId);
}
