using FloppyBot.Aux.TwitchAlerts.Core.Entities.Storage;
using FloppyBot.Base.Storage;

namespace FloppyBot.Aux.TwitchAlerts.Core.Entities;

public interface ITwitchAlertService
{
    TwitchAlertSettings? GetAlertSettings(string channelId);
    void StoreAlertSettings(TwitchAlertSettings settings);
}

public class TwitchAlertService : ITwitchAlertService
{
    private readonly IRepository<TwitchAlertSettingsEo> _repository;

    public TwitchAlertService(IRepositoryFactory repositoryFactory)
    {
        _repository = repositoryFactory.GetRepository<TwitchAlertSettingsEo>();
    }

    public TwitchAlertSettings? GetAlertSettings(string channelId)
    {
        return _repository.GetById(channelId).ToDto();
    }

    public void StoreAlertSettings(TwitchAlertSettings settings)
    {
        _repository.Upsert(settings.ToEo());
    }
}
