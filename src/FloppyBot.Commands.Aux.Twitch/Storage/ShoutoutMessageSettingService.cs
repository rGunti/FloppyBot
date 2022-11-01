using FloppyBot.Base.Storage;
using FloppyBot.Commands.Aux.Twitch.Storage.Entities;

namespace FloppyBot.Commands.Aux.Twitch.Storage;

public class ShoutoutMessageSettingService : IShoutoutMessageSettingService
{
    private readonly IRepository<ShoutoutMessageSetting> _repository;

    public ShoutoutMessageSettingService(IRepositoryFactory repositoryFactory)
    {
        _repository = repositoryFactory.GetRepository<ShoutoutMessageSetting>();
    }

    public ShoutoutMessageSetting? GetSettings(string channelId)
    {
        return _repository.GetById(channelId);
    }

    public void SetShoutoutMessage(string channelId, string message)
    {
        var existingSettings = GetSettings(channelId);
        if (existingSettings != null)
        {
            _repository.Update(existingSettings with
            {
                Message = message
            });
        }
        else
        {
            _repository.Insert(new ShoutoutMessageSetting(
                channelId,
                message));
        }
    }

    public void ClearSettings(string channelId)
    {
        _repository.Delete(channelId);
    }
}
