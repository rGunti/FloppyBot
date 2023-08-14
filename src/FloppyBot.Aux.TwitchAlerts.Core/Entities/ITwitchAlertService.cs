using AutoMapper;
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
    private readonly IMapper _mapper;

    public TwitchAlertService(IRepositoryFactory repositoryFactory, IMapper mapper)
    {
        _mapper = mapper;
        _repository = repositoryFactory.GetRepository<TwitchAlertSettingsEo>();
    }

    public TwitchAlertSettings? GetAlertSettings(string channelId)
    {
        return _mapper.Map<TwitchAlertSettings>(_repository.GetById(channelId));
    }

    public void StoreAlertSettings(TwitchAlertSettings settings)
    {
        _repository.Upsert(_mapper.Map<TwitchAlertSettingsEo>(settings));
    }
}
